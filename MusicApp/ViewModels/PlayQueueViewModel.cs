using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GongSolutions.Wpf.DragDrop;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quan ly hang doi phat nhac ho tro keo tha sap xep (Play Queue ViewModel with Drag &amp; Drop).
    /// 
    /// Tac dung:
    /// - Quan ly danh sach cac ban nhac tiep theo se duoc phat (QueueTracks).
    /// - Hien thuc hoa giao dien IDropTarget tu thu vien GongSolutions.Wpf.DragDrop, cho phep nguoi dung
    ///   keo va tha truc quan de thay doi thu tu phat uu tien trong hang doi.
    /// - Tinh toan tong so luong bai hat (QueueCount) va tong thoi luong phat (TotalQueueDuration) tu dong.
    /// - Cung cap cac thao tac: Enqueue, DequeueNext, Remove, Move, Clear, PlayNow.
    /// 
    /// Van de giai quyet:
    /// - Cho phep nguoi dung chu dong sap dat danh sach bai hat phat theo y muon ma khong lam gian doan ca khuc dang nghe.
    /// - Tu dong lay bai hat tiep theo trong hang doi (Priority 1) khi ca khuc hien tai ket thuc truoc khi fallback sang danh sach chung.
    /// - Cap nhat so lieu thong ke (so bai, tong thoi luong) tuc thi nho su kien CollectionChanged cua ObservableCollection.
    /// 
    /// Cach thuc van hanh:
    /// - GongSolutions.Wpf.DragDrop goi DragOver va Drop de di chuyen item trong collection.
    /// - Ham DequeueNext lay bai hat dau tien o vi tri 0 (FIFO), xoa khoi hang doi va tra ve cho NowPlayingViewModel phat.
    /// </summary>
    public class PlayQueueViewModel : ObservableObject, IDropTarget
    {
        private readonly Action<TrackModel> _onPlayTrack;

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
        public PlayQueueViewModel(Action<TrackModel> onPlayTrack)
        {
            _onPlayTrack = onPlayTrack;

            QueueTracks.CollectionChanged += OnQueueTracksCollectionChanged;

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
        }

        /// <summary>
        /// Lay ra va xoa ban nhac dau tien trong hang doi (FIFO) de bat dau phat.
        /// </summary>
        /// <returns>Doi tuong TrackModel dau hang doi, hoac null neu hang doi rong.</returns>
        public TrackModel DequeueNext()
        {
            if (QueueTracks.Count == 0) return null;

            var first = QueueTracks[0];
            QueueTracks.RemoveAt(0);
            return first.Track;
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
            }
        }

        /// <summary>
        /// Xoa sach toan bo danh sach bai hat trong hang doi.
        /// </summary>
        public void Clear()
        {
            QueueTracks.Clear();
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
        }
    }
}
