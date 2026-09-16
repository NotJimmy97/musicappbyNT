using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quan ly hien thi va dong bo loi bai hat (Synchronized Lyrics ViewModel).
    /// 
    /// Tac dung:
    /// - Nap va quan ly danh sach cac dong loi LyricLineViewModel cho ca khuc dang duoc phat hien tai.
    /// - Dong bo hoa thoi gian thuc giua vi tri phat am thanh cua IAudioService va dong loi tuong ung.
    /// - Phat su kien ActiveLineChanged de giao dien LyricsSyncView.xaml.cs tu dong cuon man hinh (Auto-scroll).
    /// - Cung cap cac trang thai IsLoading va HasLyrics giup hien thi Empty State thich hop khi khong co loi.
    /// 
    /// Van de giai quyet:
    /// - Thuat toan tim kiem nhi phan toi uu O(log N): Thay vi duyet tuyen tinh O(N) qua hang tram dong loi moi 250ms,
    ///   UpdatePosition su dung thuat toan Binary Search tim ra dong loi co Timestamp &lt;= vi tri hien tai,
    ///   giup tiet kiem CPU toi da khi phat nhac.
    /// - Bo loc chuyen doi trang thai (State Transition Gate): Chi cap nhat thuoc tinh IsActive va thong bao len UI
    ///   khi vi tri dong loi THUC SU thay doi (candidate != _activeLineIndex), tranh gay giat man hinh do trigger layout lai lien tuc.
    /// - Quan ly Task an toan thong qua CancellationTokenSource: Tu dong huy bo tac vu nap loi cu neu nguoi dung chuyen bai lien tiep.
    /// 
    /// Cach thuc van hanh:
    /// - Khi MainViewModel phat bai hat moi, goi LoadLyricsForTrackAsync.
    /// - Dong ho vi tri cua NowPlayingViewModel lien tuc goi UpdatePosition(currentTime).
    /// - Khi dong loi hat thay doi, activeLine.IsActive = true va kich hoat su kien ActiveLineChanged.
    /// </summary>
    public class LyricsViewModel : ObservableObject
    {
        private readonly ILyricsService _lyricsService;
        private readonly Action<TimeSpan> _onSeek;
        private CancellationTokenSource _loadCts;

        /// <summary>
        /// Tap hop cac dong loi bai hat hien thi tren danh sach ListBox.
        /// </summary>
        public ObservableCollection<LyricLineViewModel> Lines { get; } = new ObservableCollection<LyricLineViewModel>();

        private TrackModel _currentTrack;

        /// <summary>
        /// Ban nhac dang duoc hien thi loi.
        /// </summary>
        public TrackModel CurrentTrack
        {
            get => _currentTrack;
            private set
            {
                if (SetProperty(ref _currentTrack, value))
                {
                    OnPropertyChanged(nameof(CurrentSongTitle));
                    OnPropertyChanged(nameof(CurrentSongArtist));
                }
            }
        }

        /// <summary>
        /// Tieu de bai hat hien tai.
        /// </summary>
        public string CurrentSongTitle => CurrentTrack?.Title ?? "Chưa chọn bài hát";

        /// <summary>
        /// Ten ca si the hien bai hat.
        /// </summary>
        public string CurrentSongArtist => CurrentTrack?.Artist ?? "Chọn một bài hát để xem lời đồng bộ";

        private bool _hasLyrics;

        /// <summary>
        /// Xac dinh ca khuc co ton tai loi bai hat hop le hay khong.
        /// </summary>
        public bool HasLyrics
        {
            get => _hasLyrics;
            private set => SetProperty(ref _hasLyrics, value);
        }

        private bool _isLoading;

        /// <summary>
        /// Trang thai dang nap du lieu loi bai hat tu dia hoac bo nho.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        private int _activeLineIndex = -1;

        /// <summary>
        /// Chi so thu tu cua dong loi dang duoc phat hien tai (0..N-1).
        /// </summary>
        public int ActiveLineIndex
        {
            get => _activeLineIndex;
            private set
            {
                if (SetProperty(ref _activeLineIndex, value))
                {
                    OnPropertyChanged(nameof(ActiveLine));
                }
            }
        }

        /// <summary>
        /// Doi tuong LyricLineViewModel cua dong loi hat dang duoc kich hoat.
        /// </summary>
        public LyricLineViewModel ActiveLine => (_activeLineIndex >= 0 && _activeLineIndex < Lines.Count) ? Lines[_activeLineIndex] : null;

        /// <summary>
        /// Su kien thong bao khi chi so dong loi hat thay doi, phuc vu ScrollViewer trong code-behind.
        /// </summary>
        public event EventHandler<int> ActiveLineChanged;

        /// <summary>
        /// Khoi tao LyricsViewModel voi dich vu doc loi va callback tua am thanh.
        /// </summary>
        /// <param name="lyricsService">Dich vu trich xuat va parse loi bai hat.</param>
        /// <param name="onSeek">Callback thuc hien lenh tua am thanh.</param>
        public LyricsViewModel(ILyricsService lyricsService, Action<TimeSpan> onSeek)
        {
            _lyricsService = lyricsService ?? throw new ArgumentNullException(nameof(lyricsService));
            _onSeek = onSeek;
        }

        /// <summary>
        /// Nap loi bai hat bat dong bo cho ban nhac chi dinh.
        /// </summary>
        /// <param name="track">Ban nhac can nap loi.</param>
        public async Task LoadLyricsForTrackAsync(TrackModel track)
        {
            // Huy bo tac vu nap loi dang chay truoc do neu co
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;

            CurrentTrack = track;
            ActiveLineIndex = -1;

            if (track == null)
            {
                Lines.Clear();
                HasLyrics = false;
                return;
            }

            IsLoading = true;

            try
            {
                var parsedLines = await _lyricsService.LoadLyricsForTrackAsync(track, token).ConfigureAwait(false);

                if (token.IsCancellationRequested) return;

                Action updateAction = () =>
                {
                    Lines.Clear();
                    for (int i = 0; i < parsedLines.Count; i++)
                    {
                        Lines.Add(new LyricLineViewModel(parsedLines[i], _onSeek));
                    }
                    HasLyrics = Lines.Count > 0;
                    ActiveLineIndex = -1;
                };

                // Dam bao cap nhat ObservableCollection tren UI Dispatcher Thread
                var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(updateAction);
                }
                else
                {
                    updateAction();
                }
            }
            catch (Exception)
            {
                Lines.Clear();
                HasLyrics = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cap nhat moc thoi gian phat am thanh va xac dinh dong loi tuong ung bang Binary Search O(log N).
        /// </summary>
        /// <param name="position">Vi tri phat am thanh hien tai.</param>
        public void UpdatePosition(TimeSpan position)
        {
            if (Lines.Count == 0) return;

            // Thuat toan tim kiem nhi phan vi tri lon nhat co Timestamp <= vi tri hien tai
            int low = 0;
            int high = Lines.Count - 1;
            int candidate = -1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (Lines[mid].Timestamp <= position)
                {
                    candidate = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            // Chi kich hoat cap nhat Visual Tree khi dong loi hat thuc su thay doi
            if (candidate != _activeLineIndex)
            {
                int prev = _activeLineIndex;
                if (prev >= 0 && prev < Lines.Count)
                {
                    Lines[prev].IsActive = false;
                }

                ActiveLineIndex = candidate;

                if (candidate >= 0 && candidate < Lines.Count)
                {
                    Lines[candidate].IsActive = true;
                }

                ActiveLineChanged?.Invoke(this, candidate);
            }
        }
    }
}
