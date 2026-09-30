using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;
using MusicApp.Core.Services;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel điều khiển phát nhạc và hiển thị visualizer.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Cung cấp commands (Play/Pause/Seek), đồng bộ UI slider và xử lý phổ tần số âm thanh.
    /// 2. Không chịu trách nhiệm: Quản lý hàng đợi (PlayQueue), gọi API lấy nhạc.
    /// 3. Vòng đời: Singleton/Tồn tại suốt vòng đời ứng dụng.
    /// 4. Đa luồng: Sự kiện SpectrumDataReady và AudioStateChanged bắn từ luồng Audio, phải InvokeAsync sang UI thread.
    /// State transitions:
    /// - Khi đổi bài: Đặt lại CurrentPositionSeconds = 0, cập nhật TrackDuration.
    /// - Khi AudioStateChanged báo Stopped (do hết bài): Kích hoạt PlayNextAction.
    /// - Khi play/pause: Cập nhật CanSeek, khởi động/dừng DispatcherTimer.
    /// </remarks>
    public class NowPlayingViewModel : ObservableObject, IDisposable
    {
        private readonly IAudioService _audioService;
        private readonly RecommendationEngine _recEngine;
        private readonly ITrackRepository _trackRepo;

        /// <summary>
        /// Tham chieu toi dich vu am thanh Audio Engine ben duoi.
        /// </summary>
        public IAudioService AudioService => _audioService;

        private readonly DispatcherTimer _positionTimer;
        private bool _isUserSeeking = false;

        private TrackModel _currentTrack;

        /// <summary>
        /// Ban nhac dang duoc phat hien tai.
        /// </summary>
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

        /// <summary>
        /// Trang thai hoat dong hien tai cua trinh phat am thanh.
        /// </summary>
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

        /// <summary>
        /// Xac dinh am thanh co dang thuc su phat hay khong.
        /// </summary>
        public bool IsPlaying => PlaybackState == PlaybackState.Playing;

        /// <summary>
        /// Xac dinh nguoi dung co the tua nhac tai thoi diem hien tai hay khong.
        /// </summary>
        public bool CanSeek => PlaybackState == PlaybackState.Playing || PlaybackState == PlaybackState.Paused;

        private double _currentPositionSeconds;

        /// <summary>
        /// Vi tri phat hien tai tinh theo giay.
        /// </summary>
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

        /// <summary>
        /// Tong thoi luong bai hat tinh theo giay.
        /// </summary>
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

        /// <summary>
        /// Chuoi van ban bieu dien vi tri phat hien tai (mm:ss).
        /// </summary>
        public string FormattedPosition => FormatSeconds(CurrentPositionSeconds);

        /// <summary>
        /// Chuoi van ban bieu dien tong thoi luong bai hat (mm:ss).
        /// </summary>
        public string FormattedDuration => FormatSeconds(TrackDurationSeconds);

        /// <summary>
        /// Kiem tra da co bai hat nao duoc chon hay chua.
        /// </summary>
        public bool HasTrackSelected => CurrentTrack != null;

        private bool _isSpinEnabled = true;

        /// <summary>
        /// Trang thai bat/tat hieu ung dia than xoay tron tren giao dien.
        /// </summary>
        public bool IsSpinEnabled
        {
            get => _isSpinEnabled;
            set => SetProperty(ref _isSpinEnabled, value);
        }

        private bool _isRainbowEq = true;

        /// <summary>
        /// Trang thai bat/tat mau sac cau vong cho song nhac Equalizer Visualizer.
        /// </summary>
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

        /// <summary>
        /// Muc am luong phat (0.0 den 1.0).
        /// </summary>
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

        // 16 mau sac chuyen tiep cau vong tieu chuan tu do toi tim
        private static readonly string[] SpectrumColors = new[]
        {
            "#FF0000", "#FF4000", "#FF8000", "#FFBF00",
            "#FFFF00", "#BFFF00", "#80FF00", "#40FF00",
            "#00FF00", "#00FF80", "#00FFFF", "#00BFFF",
            "#0080FF", "#0040FF", "#0000FF", "#8000FF"
        };
        private const string SpotifyGreen = "#1ED760";

        /// <summary>
        /// Tap hop 16 gia tri bien do song nhac.
        /// </summary>
        public ObservableCollection<double> EqualizerBins { get; } = new ObservableCollection<double>();

        /// <summary>
        /// Tap hop 16 ViewModel dai dien cho 16 cot song nhac hien thi tren UI.
        /// </summary>
        public ObservableCollection<EqualizerBarViewModel> EqualizerBars { get; } = new ObservableCollection<EqualizerBarViewModel>();

        /// <summary>
        /// Hanh dong goi khi can phat bai hat tiep theo.
        /// </summary>
        public Action PlayNextAction { get; set; }

        /// <summary>
        /// Hanh dong goi khi can quay lai bai hat phia truoc.
        /// </summary>
        public Action PlayPreviousAction { get; set; }

        /// <summary>
        /// Callback thong bao moc thoi gian thay doi sang LyricsViewModel.
        /// </summary>
        public Action<TimeSpan> PositionChanged { get; set; }

        /// <summary>
        /// Lenh tiep tuc phat nhac.
        /// </summary>
        public RelayCommand PlayCommand { get; }

        /// <summary>
        /// Lenh tam dung phat nhac.
        /// </summary>
        public RelayCommand PauseCommand { get; }

        /// <summary>
        /// Lenh dung han phat nhac.
        /// </summary>
        public RelayCommand StopCommand { get; }

        /// <summary>
        /// Lenh tua den moc thoi gian cu the.
        /// </summary>
        public RelayCommand SeekCommand { get; }

        /// <summary>
        /// Lenh chuyen toi bai hat tiep theo.
        /// </summary>
        public RelayCommand NextTrackCommand { get; }

        /// <summary>
        /// Lenh quay ve bai hat truoc do.
        /// </summary>
        public RelayCommand PreviousTrackCommand { get; }

        /// <summary>
        /// Lenh bat/tat hieu ung xoay dia.
        /// </summary>
        public RelayCommand ToggleSpinCommand { get; }

        /// <summary>
        /// Lenh chuyen doi giua che do mau Cau vong va mau Xanh Spotify.
        /// </summary>
        public RelayCommand ToggleRainbowCommand { get; }

        private bool _isCurrentTrackFavorite;
        public bool IsCurrentTrackFavorite
        {
            get => _isCurrentTrackFavorite;
            set => SetProperty(ref _isCurrentTrackFavorite, value);
        }

        public AsyncRelayCommand ToggleFavoriteCommand { get; }

        private int _currentDbTrackId;
        private long _playGeneration;

        private async Task<int> EnsureTrackEntityIdAsync(TrackModel track)
        {
            if (track == null || _trackRepo == null) return 0;
            if (int.TryParse(track.Id, out int id) && id > 0)
            {
                var existing = await _trackRepo.GetByIdAsync(id).ConfigureAwait(false);
                if (existing != null) return id;
            }

            string trackKey = TrackIdentityHelper.GenerateTrackKey(track.Title, track.Artist);
            var byKey = await _trackRepo.GetByTrackKeyAsync(trackKey).ConfigureAwait(false);
            if (byKey != null)
            {
                track.Id = byKey.Id.ToString();
                return byKey.Id;
            }

            var newEntity = new TrackEntity
            {
                TrackKey = trackKey,
                SourceType = track.StreamUrl != null && track.StreamUrl.StartsWith("http") ? "online" : "local",
                SourceId = track.Id,
                Title = track.Title ?? "Unknown Title",
                Artist = track.Artist ?? "Unknown Artist",
                Album = track.Album,
                Genre = track.Genre,
                DurationSeconds = track.DurationSeconds,
                CoverUri = track.CoverImageUrl,
                AffinityScore = 1.0
            };
            int newId = await _trackRepo.InsertOrUpdateAsync(newEntity).ConfigureAwait(false);
            if (newId > 0)
            {
                track.Id = newId.ToString();
            }
            return newId;
        }

        /// <summary>
        /// Khoi tao NowPlayingViewModel va cau hinh 16 cot visualizer kem bo dem timer.
        /// </summary>
        /// <param name="audioService">Dich vu am thanh Audio Engine.</param>
        /// <param name="recEngine">Dong co goi y thong minh.</param>
        /// <param name="trackRepo">Kho luu tru bai hat SQLite.</param>
        public NowPlayingViewModel(IAudioService audioService, RecommendationEngine recEngine = null, ITrackRepository trackRepo = null)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _recEngine = recEngine;
            _trackRepo = trackRepo;

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

            NextTrackCommand = new RelayCommand(_ =>
            {
                // Neu skip truoc 30 giay, ghi nhan hanh vi skip de ha diem affinity
                if (_currentDbTrackId > 0 && CurrentPositionSeconds < 30)
                {
                    int trackId = _currentDbTrackId;
                    Task.Run(() => _recEngine?.LogActionAsync(trackId, "skip", (int)CurrentPositionSeconds));
                }
                PlayNextAction?.Invoke();
            });

            PreviousTrackCommand = new RelayCommand(_ => PlayPreviousAction?.Invoke());
            ToggleSpinCommand = new RelayCommand(_ => IsSpinEnabled = !IsSpinEnabled);
            ToggleRainbowCommand = new RelayCommand(_ => IsRainbowEq = !IsRainbowEq);

            ToggleFavoriteCommand = new AsyncRelayCommand(async _ =>
            {
                if (_currentDbTrackId > 0)
                {
                    int trackId = _currentDbTrackId;
                    IsCurrentTrackFavorite = !IsCurrentTrackFavorite;
                    if (_trackRepo != null)
                    {
                        await _trackRepo.ToggleFavoriteAsync(trackId).ConfigureAwait(false);
                    }
                    if (_recEngine != null)
                    {
                        await _recEngine.LogActionAsync(trackId, IsCurrentTrackFavorite ? "favorite" : "unfavorite").ConfigureAwait(false);
                    }
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

        /// <summary>
        /// Cap nhat mau sac cho 16 cot visualizer dua tren cau hinh IsRainbowEq.
        /// </summary>
        private void ApplyEqualizerColors()
        {
            for (int i = 0; i < EqualizerBars.Count; i++)
            {
                EqualizerBars[i].ColorHex = IsRainbowEq ? SpectrumColors[i % SpectrumColors.Length] : SpotifyGreen;
            }
        }

        /// <summary>
        /// Khoi tao va bat dau phat mot ban nhac bat dong bo tren Audio Engine.
        /// </summary>
        /// <param name="track">Ban nhac can phat.</param>
        public async Task PlayTrackAsync(TrackModel track)
        {
            if (track == null) return;

            var generation = Interlocked.Increment(ref _playGeneration);
            CurrentTrack = track;
            CurrentPositionSeconds = 0;

            if (_trackRepo != null)
            {
                _ = Task.Run(async () =>
                {
                    int dbId = await EnsureTrackEntityIdAsync(track).ConfigureAwait(false);
                    if (Interlocked.Read(ref _playGeneration) != generation) return;

                    _currentDbTrackId = dbId;
                    if (dbId > 0)
                    {
                        var entity = await _trackRepo.GetByIdAsync(dbId).ConfigureAwait(false);
                        if (entity != null && Interlocked.Read(ref _playGeneration) == generation)
                        {
                            var disp = Application.Current?.Dispatcher;
                            if (disp != null)
                            {
                                await disp.InvokeAsync(() => IsCurrentTrackFavorite = entity.IsFavorite);
                            }
                        }
                    }
                });
            }

            try
            {
                await _audioService.InitializeAsync(track.StreamUrl).ConfigureAwait(true);
                if (Interlocked.Read(ref _playGeneration) != generation) return;
                _audioService.Play();
            }
            catch (Exception ex)
            {
                if (Interlocked.Read(ref _playGeneration) != generation) return;
                PlaybackState = PlaybackState.Faulted;
                MessageBox.Show($"Lỗi khởi tạo âm thanh: {ex.Message}", "Lỗi Phát Nhạc", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xac dinh trang thai nguoi dung dang keo chuot tren thanh truot thoi gian de tranh xung dot timer.
        /// </summary>
        /// <param name="isSeeking">Co keo chuot.</param>
        /// <param name="targetSeconds">Moc thoi gian tha chuot.</param>
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

        /// <summary>
        /// Dong ho dinh ky 250ms doc vi tri phat am thanh hien tai va dong bo sang giao dien va Lyrics.
        /// </summary>
        private void OnPositionTimerTick(object sender, EventArgs e)
        {
            if (!_isUserSeeking && _audioService != null)
            {
                var currentTime = _audioService.CurrentTime;
                CurrentPositionSeconds = currentTime.TotalSeconds;
                PositionChanged?.Invoke(currentTime);
            }
        }

        /// <summary>
        /// Xu ly khi trang thai van hanh cua Audio Engine thay doi.
        /// Tu dong kich hoat phat bai tiep theo neu bai hien tai da phat den het.
        /// </summary>
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
                        if (_currentDbTrackId > 0)
                        {
                            int trackId = _currentDbTrackId;
                            Task.Run(() => _recEngine?.LogActionAsync(trackId, "play_complete", (int)TrackDurationSeconds));
                        }
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

        /// <summary>
        /// Nhan du lieu bien do 16 dai tan so tu Audio Engine va cap nhat lenh render Visualizer.
        /// </summary>
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

        /// <summary>
        /// Dat lai toan bo cac cot visualizer ve chieu cao co so 2.0 khi dung nhac.
        /// </summary>
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

        /// <summary>
        /// Dinh dang so giay thanh chuoi phut giay (mm:ss hoac hh:mm:ss).
        /// </summary>
        private static string FormatSeconds(double totalSeconds)
        {
            var span = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
            return span.TotalHours >= 1 ? span.ToString(@"hh\:mm\:ss") : span.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Giai phong timer va huy dang ky toan bo su kien am thanh khi viewmodel bi tieu huy.
        /// </summary>
        public void Dispose()
        {
            _positionTimer.Stop();
            _audioService.SpectrumDataReady -= OnSpectrumDataReady;
            _audioService.StateChanged -= OnAudioStateChanged;
        }
    }
}
