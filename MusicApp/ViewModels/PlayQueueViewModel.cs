using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GongSolutions.Wpf.DragDrop;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;
using MusicApp.Core.Services;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quản lý hàng đợi phát nhạc hỗ trợ kéo thả (Drag & Drop).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Quản lý danh sách QueueTracks, xử lý logic Enqueue/Dequeue, lưu trữ trạng thái xuống SQLite.
    /// 2. Không chịu trách nhiệm: Trực tiếp phát nhạc (chỉ gọi delegate OnPlayTrack).
    /// 3. Vòng đời: Tồn tại suốt vòng đời ứng dụng.
    /// 4. Đa luồng: Tự động update trên UI thread thông qua Dispatcher khi RestoreQueueFromDatabaseAsync.
    /// State transitions:
    /// - Khi QueueTracks thay đổi: Tự động tính lại tổng thời lượng và gọi PersistQueueToDatabase.
    /// - Khi rút bài (DequeueNext): Chạy theo FIFO hoặc ưu tiên Smart Shuffle nếu được bật.
    /// </remarks>
    public class PlayQueueViewModel : ObservableObject, IDropTarget
    {
        private readonly Action<TrackModel> _onPlayTrack;
        private readonly IQueueRepository _queueRepo;
        private readonly RecommendationEngine _recEngine;
        private readonly SemaphoreSlim _queueSaveLock = new SemaphoreSlim(1, 1);

        private bool _isSmartShuffle;
        public bool IsSmartShuffle
        {
            get => _isSmartShuffle;
            set => SetProperty(ref _isSmartShuffle, value);
        }

        public RelayCommand ToggleSmartShuffleCommand { get; }

        /// <summary>
        /// Danh sach cac bai hat dang nam trong hang doi phat.
        /// </summary>
        public ObservableCollection<TrackItemViewModel> QueueTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        /// <summary>
        /// Tong so luong bai hat hien co trong hang doi.
        /// </summary>
        public int QueueCount => QueueTracks.Count;

        private TimeSpan _totalQueueDuration = TimeSpan.Zero;

        /// <summary>
        /// Tong thoi luong phat cua toan bo hang doi.
        /// </summary>
        public TimeSpan TotalQueueDuration
        {
            get => _totalQueueDuration;
            private set
            {
                if (SetProperty(ref _totalQueueDuration, value))
                {
                    OnPropertyChanged(nameof(FormattedQueueDuration));
                }
            }
        }

        /// <summary>
        /// Chuoi van ban bieu dien tong thoi luong da duoc dinh dang (hh:mm:ss hoac mm:ss).
        /// </summary>
        public string FormattedQueueDuration
        {
            get
            {
                if (TotalQueueDuration.TotalHours >= 1)
                {
                    return string.Format("{0:D2}:{1:D2}:{2:D2}", 
                        (int)TotalQueueDuration.TotalHours, 
                        TotalQueueDuration.Minutes, 
                        TotalQueueDuration.Seconds);
                }
                return string.Format("{0:D2}:{1:D2}", 
                    TotalQueueDuration.Minutes, 
                    TotalQueueDuration.Seconds);
            }
        }

        private TrackItemViewModel _selectedQueueTrack;

        /// <summary>
        /// Ban nhac dang duoc chon trong danh sach hang doi.
        /// </summary>
        public TrackItemViewModel SelectedQueueTrack
        {
            get => _selectedQueueTrack;
            set => SetProperty(ref _selectedQueueTrack, value);
        }

        /// <summary>
        /// Lenh dua mot ban nhac vao cuoi hang doi phat.
        /// </summary>
        public RelayCommand EnqueueCommand { get; }

        /// <summary>
        /// Lenh xoa mot ban nhac khoi hang doi phat.
        /// </summary>
        public RelayCommand RemoveFromQueueCommand { get; }

        /// <summary>
        /// Lenh xoa toan bo tat ca bai hat trong hang doi.
        /// </summary>
        public RelayCommand ClearQueueCommand { get; }

        /// <summary>
        /// Lenh phat ngay mot bai hat trong hang doi (xoa khoi hang doi va bat dau phat).
        /// </summary>
        public RelayCommand PlayNowFromQueueCommand { get; }

        /// <summary>
        /// Lenh phat bai hat tiep theo o dau hang doi.
        /// </summary>
        public RelayCommand PlayNextInQueueCommand { get; }

        /// <summary>
        /// Khoi tao PlayQueueViewModel voi callback phat nhac va dang ky su kien thay doi danh sach.
        /// </summary>
        /// <param name="onPlayTrack">Callback phat bai hat.</param>
        /// <param name="queueRepo">Kho luu tru hang doi SQLite.</param>
        /// <param name="recEngine">Dong co de xuat thong minh.</param>
        public PlayQueueViewModel(Action<TrackModel> onPlayTrack, IQueueRepository queueRepo = null, RecommendationEngine recEngine = null)
        {
            _onPlayTrack = onPlayTrack;
            _queueRepo = queueRepo;
            _recEngine = recEngine;

            QueueTracks.CollectionChanged += OnQueueTracksCollectionChanged;

            ToggleSmartShuffleCommand = new RelayCommand(_ => IsSmartShuffle = !IsSmartShuffle);

            EnqueueCommand = new RelayCommand(p =>
            {
                if (p is TrackModel track)
                {
                    Enqueue(track);
                }
                else if (p is TrackItemViewModel itemVm)
                {
                    Enqueue(itemVm.Track);
                }
            });

            RemoveFromQueueCommand = new RelayCommand(p =>
            {
                if (p is TrackItemViewModel itemVm)
                {
                    Remove(itemVm);
                }
            });

            ClearQueueCommand = new RelayCommand(_ => Clear(), _ => QueueTracks.Count > 0);

            PlayNowFromQueueCommand = new RelayCommand(p =>
            {
                if (p is TrackItemViewModel itemVm)
                {
                    Remove(itemVm);
                    _onPlayTrack?.Invoke(itemVm.Track);
                }
            });

            PlayNextInQueueCommand = new RelayCommand(_ =>
            {
                var next = DequeueNext();
                if (next != null)
                {
                    _onPlayTrack?.Invoke(next);
                }
            }, _ => QueueTracks.Count > 0);
        }

        /// <summary>
        /// Them mot ban nhac vao cuoi hang doi.
        /// </summary>
        /// <param name="track">Ban nhac can dua vao hang doi.</param>
        public void Enqueue(TrackModel track)
        {
            if (track == null) return;

            var itemVm = new TrackItemViewModel(track, t =>
            {
                var found = QueueTracks.FirstOrDefault(q => q.Track.Id == t.Id);
                if (found != null)
                {
                    QueueTracks.Remove(found);
                }
                _onPlayTrack?.Invoke(t);
            });

            QueueTracks.Add(itemVm);
            PersistQueueToDatabase();
        }

        /// <summary>
        /// Lay ra va xoa ban nhac tiep theo trong hang doi de bat dau phat.
        /// Neu Smart Shuffle duoc bat, lua chon dua tren phan phoi xac suat Boltzmann.
        /// </summary>
        /// <returns>Doi tuong TrackModel tiep theo, hoac null neu hang doi rong.</returns>
        public TrackModel DequeueNext(int currentTrackId = 0)
        {
            if (QueueTracks.Count == 0) return null;

            if (IsSmartShuffle && _recEngine != null && QueueTracks.Count > 1)
            {
                var pool = QueueTracks.Select(q =>
                {
                    int.TryParse(q.Track.Id, out int id);
                    return new TrackEntity
                    {
                        Id = id,
                        Title = q.Track.Title,
                        Artist = q.Track.Artist
                    };
                }).ToList();

                var chosen = _recEngine.PickSmartShuffleNext(pool, currentTrackId);
                if (chosen != null)
                {
                    var found = QueueTracks.FirstOrDefault(q => q.Track.Id == chosen.Id.ToString()) ?? QueueTracks[0];
                    QueueTracks.Remove(found);
                    PersistQueueToDatabase();
                    return found.Track;
                }
            }

            var first = QueueTracks[0];
            QueueTracks.RemoveAt(0);
            PersistQueueToDatabase();
            return first.Track;
        }

        /// <summary>
        /// Luu tru danh sach ID cac bai hat trong hang doi vao SQLite bat dong bo, tuan tu hoa qua SemaphoreSlim.
        /// </summary>
        public async Task PersistQueueToDatabaseAsync()
        {
            if (_queueRepo == null) return;
            var ids = QueueTracks
                .Select(q => int.TryParse(q.Track.Id, out int id) ? (int?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            await _queueSaveLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await _queueRepo.SaveQueueAsync(ids).ConfigureAwait(false);
            }
            finally
            {
                _queueSaveLock.Release();
            }
        }

        /// <summary>
        /// Luu tru danh sach ID cac bai hat trong hang doi vao SQLite (fire-and-forget an toan).
        /// </summary>
        public void PersistQueueToDatabase()
        {
            Task.Run(async () => await PersistQueueToDatabaseAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Khoi phuc hang doi tu SQLite khi ung dung khoi dong.
        /// </summary>
        public async Task RestoreQueueFromDatabaseAsync()
        {
            if (_queueRepo == null) return;
            try
            {
                var entities = await _queueRepo.LoadQueueAsync().ConfigureAwait(false);
                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                Action populate = () =>
                {
                    foreach (var e in entities)
                    {
                        var track = new TrackModel
                        {
                            Id = e.Id.ToString(),
                            Title = e.Title,
                            Artist = e.Artist,
                            Album = e.Album,
                            CoverImageUrl = e.CoverUri,
                            StreamUrl = e.SourceType == "local"
                                ? e.SourceId
                                : (e.SourceId != null && e.SourceId.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                    ? e.SourceId
                                    : $"http://localhost:5245/api/v1/stream/{e.SourceId}"),
                            Genre = e.Genre
                        };
                        var itemVm = new TrackItemViewModel(track, t =>
                        {
                            var found = QueueTracks.FirstOrDefault(q => q.Track.Id == t.Id);
                            if (found != null) QueueTracks.Remove(found);
                            _onPlayTrack?.Invoke(t);
                        });
                        QueueTracks.Add(itemVm);
                    }
                };

                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(populate);
                }
                else
                {
                    populate();
                }
            }
            catch
            {
                // Bo qua loi nap queue khi khoi dong
            }
        }

        /// <summary>
        /// Xoa mot phan tu TrackItemViewModel khoi hang doi.
        /// </summary>
        /// <param name="item">Phan tu can xoa.</param>
        public void Remove(TrackItemViewModel item)
        {
            if (item != null && QueueTracks.Contains(item))
            {
                QueueTracks.Remove(item);
                PersistQueueToDatabase();
            }
        }

        /// <summary>
        /// Di chuyen mot ban nhac tu vi tri cu sang vi tri moi trong hang doi.
        /// </summary>
        /// <param name="oldIndex">Vi tri ban dau.</param>
        /// <param name="newIndex">Vi tri moi.</param>
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < QueueTracks.Count &&
                newIndex >= 0 && newIndex < QueueTracks.Count &&
                oldIndex != newIndex)
            {
                QueueTracks.Move(oldIndex, newIndex);
                PersistQueueToDatabase();
            }
        }

        /// <summary>
        /// Xoa sach toan bo danh sach bai hat trong hang doi.
        /// </summary>
        public void Clear()
        {
            QueueTracks.Clear();
            PersistQueueToDatabase();
        }

        /// <summary>
        /// Xu ly su kien khi cac phan tu trong QueueTracks thay doi de cap nhat lai so lieu thong ke.
        /// </summary>
        private void OnQueueTracksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateQueueStats();
        }

        /// <summary>
        /// Tinh toan lai tong thoi luong va so luong bai hat trong hang doi.
        /// </summary>
        public void UpdateQueueStats()
        {
            int totalSeconds = 0;
            for (int i = 0; i < QueueTracks.Count; i++)
            {
                totalSeconds += QueueTracks[i].DurationSeconds;
            }

            TotalQueueDuration = TimeSpan.FromSeconds(totalSeconds);
            OnPropertyChanged(nameof(QueueCount));
            ClearQueueCommand?.RaiseCanExecuteChanged();
            PlayNextInQueueCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Xu ly su kien keo qua (DragOver) cua thu vien GongSolutions.Wpf.DragDrop.
        /// </summary>
        public void DragOver(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }

        /// <summary>
        /// Xu ly su kien tha phan tu (Drop) cua thu vien GongSolutions.Wpf.DragDrop de cap nhat vi tri va thong ke.
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            UpdateQueueStats();
            PersistQueueToDatabase();
        }
    }
}
