using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SpotifyWpf.Core.Common;
using SpotifyWpf.Core.Interfaces;
using SpotifyWpf.Core.Models;

namespace SpotifyWpf.Client.ViewModels
{
    public class NowPlayingViewModel : ObservableObject, IDisposable
    {
        private readonly IAudioService _audioService;
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

        public ObservableCollection<double> EqualizerBins { get; } = new ObservableCollection<double>();

        public RelayCommand PlayCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand SeekCommand { get; }

        public NowPlayingViewModel(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));

            for (int i = 0; i < 16; i++)
            {
                EqualizerBins.Add(0.0);
            }

            PlayCommand = new RelayCommand(_ => _audioService.Play(), _ => PlaybackState == PlaybackState.Paused || PlaybackState == PlaybackState.Stopped);
            PauseCommand = new RelayCommand(_ => _audioService.Pause(), _ => PlaybackState == PlaybackState.Playing);
            StopCommand = new RelayCommand(_ => _audioService.Stop(), _ => PlaybackState == PlaybackState.Playing || PlaybackState == PlaybackState.Paused);
            SeekCommand = new RelayCommand(p =>
            {
                if (p is double seconds)
                {
                    _audioService.Seek(TimeSpan.FromSeconds(seconds));
                }
            });

            _audioService.SpectrumDataReady += OnSpectrumDataReady;
            _audioService.StateChanged += OnAudioStateChanged;

            _positionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _positionTimer.Tick += OnPositionTimerTick;
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
                _audioService.Seek(TimeSpan.FromSeconds(targetSeconds));
            }
        }

        private void OnPositionTimerTick(object sender, EventArgs e)
        {
            if (!_isUserSeeking && _audioService != null)
            {
                CurrentPositionSeconds = _audioService.CurrentTime.TotalSeconds;
            }
        }

        private void OnAudioStateChanged(object sender, PlaybackState state)
        {
            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                PlaybackState = state;
            });
        }

        private void OnSpectrumDataReady(object sender, float[] bins)
        {
            if (bins == null || bins.Length < 16 || PlaybackState != PlaybackState.Playing)
            {
                return;
            }

            // Update equalizer bars on Render priority to maintain fluid 30fps animation without UI lag
            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                for (int i = 0; i < 16; i++)
                {
                    EqualizerBins[i] = bins[i];
                }
            }, DispatcherPriority.Render);
        }

        private void ResetEqualizer()
        {
            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                for (int i = 0; i < 16; i++)
                {
                    EqualizerBins[i] = 0.0;
                }
            });
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
