using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    public class NowPlayingViewModel : ObservableObject, IDisposable
    {
        private readonly IAudioService _audioService;
        public IAudioService AudioService => _audioService;
        private readonly DispatcherTimer _positionTimer;
        private bool _isUserSeeking = false;

        private TrackModel _currentTrack;
        public TrackModel CurrentTrack
        {
            get => _currentTrack;
            set
            {
                if (SetProperty(ref _currentTrack, value))
                {
                    TrackDurationSeconds = value?.DurationSeconds ?? 0;
                    OnPropertyChanged(nameof(FormattedDuration));
                }
            }
        }

        private PlaybackState _playbackState = PlaybackState.Stopped;
        public PlaybackState PlaybackState
        {
            get => _playbackState;
            set
            {
                if (SetProperty(ref _playbackState, value))
                {
                    PlayCommand.RaiseCanExecuteChanged();
                    PauseCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(CanSeek));

                    if (value == PlaybackState.Playing)
                    {
                        _positionTimer.Start();
                    }
                    else
                    {
                        _positionTimer.Stop();
                        if (value == PlaybackState.Stopped)
                        {
                            ResetEqualizer();
                        }
                    }
                }
            }
        }

        public bool IsPlaying => PlaybackState == PlaybackState.Playing;
        public bool CanSeek => PlaybackState == PlaybackState.Playing || PlaybackState == PlaybackState.Paused;

        private double _currentPositionSeconds;
        public double CurrentPositionSeconds
        {
            get => _currentPositionSeconds;
            set
            {
                if (SetProperty(ref _currentPositionSeconds, value))
                {
                    OnPropertyChanged(nameof(FormattedPosition));
                }
            }
        }

        private double _trackDurationSeconds;
        public double TrackDurationSeconds
        {
            get => _trackDurationSeconds;
            set
            {
                if (SetProperty(ref _trackDurationSeconds, value))
                {
                    OnPropertyChanged(nameof(FormattedDuration));
                }
            }
        }

        public string FormattedPosition => FormatSeconds(CurrentPositionSeconds);
        public string FormattedDuration => FormatSeconds(TrackDurationSeconds);

        public bool HasTrackSelected => CurrentTrack != null;

        private bool _isSpinEnabled = true;
        public bool IsSpinEnabled
        {
            get => _isSpinEnabled;
            set => SetProperty(ref _isSpinEnabled, value);
        }

        private bool _isRainbowEq = true;
        public bool IsRainbowEq
        {
            get => _isRainbowEq;
            set
            {
                if (SetProperty(ref _isRainbowEq, value))
                {
                    ApplyEqualizerColors();
                }
            }
        }

        private double _volume = 0.8;
        public double Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, Math.Max(0.0, Math.Min(1.0, value))))
                {
                    if (_audioService != null)
                    {
                        _audioService.SetVolume((float)value);
                    }
                }
            }
        }

        // 16 rainbow spectrum colors matching tthn0/Spotify-Readme modules/colors.py
        private static readonly string[] SpectrumColors = new[]
        {
            "#FF0000", "#FF4000", "#FF8000", "#FFBF00",
            "#FFFF00", "#BFFF00", "#80FF00", "#40FF00",
            "#00FF00", "#00FF80", "#00FFFF", "#00BFFF",
            "#0080FF", "#0040FF", "#0000FF", "#8000FF"
        };
        private const string SpotifyGreen = "#1ED760";

        public ObservableCollection<double> EqualizerBins { get; } = new ObservableCollection<double>();
        public ObservableCollection<EqualizerBarViewModel> EqualizerBars { get; } = new ObservableCollection<EqualizerBarViewModel>();

        public Action PlayNextAction { get; set; }
        public Action PlayPreviousAction { get; set; }
        public Action<TimeSpan> PositionChanged { get; set; }

        public RelayCommand PlayCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand SeekCommand { get; }
        public RelayCommand NextTrackCommand { get; }
        public RelayCommand PreviousTrackCommand { get; }
        public RelayCommand ToggleSpinCommand { get; }
        public RelayCommand ToggleRainbowCommand { get; }

        public NowPlayingViewModel(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));

            for (int i = 0; i < 16; i++)
            {
                EqualizerBins.Add(0.0);
                EqualizerBars.Add(new EqualizerBarViewModel(2.0, SpectrumColors[i]));
            }

            PlayCommand = new RelayCommand(_ => _audioService.Play(), _ => PlaybackState == PlaybackState.Paused || PlaybackState == PlaybackState.Stopped);
            PauseCommand = new RelayCommand(_ => _audioService.Pause(), _ => PlaybackState == PlaybackState.Playing);
            StopCommand = new RelayCommand(_ => _audioService.Stop(), _ => PlaybackState == PlaybackState.Playing || PlaybackState == PlaybackState.Paused);
            SeekCommand = new RelayCommand(p =>
            {
                if (p is double seconds)
                {
                    var target = TimeSpan.FromSeconds(seconds);
                    _audioService.Seek(target);
                    PositionChanged?.Invoke(target);
                }
            });

            NextTrackCommand = new RelayCommand(_ => PlayNextAction?.Invoke());
            PreviousTrackCommand = new RelayCommand(_ => PlayPreviousAction?.Invoke());
            ToggleSpinCommand = new RelayCommand(_ => IsSpinEnabled = !IsSpinEnabled);
            ToggleRainbowCommand = new RelayCommand(_ => IsRainbowEq = !IsRainbowEq);

            _audioService.SpectrumDataReady += OnSpectrumDataReady;
            _audioService.StateChanged += OnAudioStateChanged;

            _positionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _positionTimer.Tick += OnPositionTimerTick;
        }

        private void ApplyEqualizerColors()
        {
            for (int i = 0; i < EqualizerBars.Count; i++)
            {
                EqualizerBars[i].ColorHex = IsRainbowEq ? SpectrumColors[i % SpectrumColors.Length] : SpotifyGreen;
            }
        }

        public async Task PlayTrackAsync(TrackModel track)
        {
            if (track == null) return;

            CurrentTrack = track;
            CurrentPositionSeconds = 0;

            try
            {
                await _audioService.InitializeAsync(track.StreamUrl).ConfigureAwait(true);
                _audioService.Play();
            }
            catch (Exception ex)
            {
                PlaybackState = PlaybackState.Faulted;
                MessageBox.Show($"Audio initialization failed: {ex.Message}", "Playback Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetUserSeeking(bool isSeeking, double targetSeconds = 0)
        {
            _isUserSeeking = isSeeking;
            if (!isSeeking && CanSeek)
            {
                var target = TimeSpan.FromSeconds(targetSeconds);
                _audioService.Seek(target);
                PositionChanged?.Invoke(target);
            }
        }

        private void OnPositionTimerTick(object sender, EventArgs e)
        {
            if (!_isUserSeeking && _audioService != null)
            {
                var currentTime = _audioService.CurrentTime;
                CurrentPositionSeconds = currentTime.TotalSeconds;
                PositionChanged?.Invoke(currentTime);
            }
        }

        private void OnAudioStateChanged(object sender, PlaybackState state)
        {
            var previous = PlaybackState;
            Action update = () =>
            {
                PlaybackState = state;
                if (previous == PlaybackState.Playing && state == PlaybackState.Stopped)
                {
                    if (TrackDurationSeconds > 0 && CurrentPositionSeconds >= Math.Max(0, TrackDurationSeconds - 2))
                    {
                        PlayNextAction?.Invoke();
                    }
                }
            };

            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.InvokeAsync(update);
            }
            else
            {
                update();
            }
        }

        private void OnSpectrumDataReady(object sender, float[] bins)
        {
            if (bins == null || bins.Length < 16 || PlaybackState != PlaybackState.Playing)
            {
                return;
            }

            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < 16; i++)
                    {
                        double val = Math.Max(2.0, (double)bins[i]);
                        EqualizerBins[i] = val;
                        if (i < EqualizerBars.Count)
                        {
                            EqualizerBars[i].Value = val;
                        }
                    }
                }, DispatcherPriority.Render);
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    double val = Math.Max(2.0, (double)bins[i]);
                    EqualizerBins[i] = val;
                    if (i < EqualizerBars.Count)
                    {
                        EqualizerBars[i].Value = val;
                    }
                }
            }
        }

        private void ResetEqualizer()
        {
            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < 16; i++)
                    {
                        EqualizerBins[i] = 2.0;
                        if (i < EqualizerBars.Count)
                        {
                            EqualizerBars[i].Value = 2.0;
                        }
                    }
                });
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    EqualizerBins[i] = 2.0;
                    if (i < EqualizerBars.Count)
                    {
                        EqualizerBars[i].Value = 2.0;
                    }
                }
            }
        }

        private static string FormatSeconds(double totalSeconds)
        {
            var span = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
            return span.TotalHours >= 1 ? span.ToString(@"hh\:mm\:ss") : span.ToString(@"mm\:ss");
        }

        public void Dispose()
        {
            _positionTimer.Stop();
            _audioService.SpectrumDataReady -= OnSpectrumDataReady;
            _audioService.StateChanged -= OnAudioStateChanged;
        }
    }
}

