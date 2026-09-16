using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GongSolutions.Wpf.DragDrop;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    public class PlayQueueViewModel : ObservableObject, IDropTarget
    {
        private readonly Action<TrackModel> _onPlayTrack;

        public ObservableCollection<TrackItemViewModel> QueueTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        public int QueueCount => QueueTracks.Count;

        private TimeSpan _totalQueueDuration = TimeSpan.Zero;
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
        public TrackItemViewModel SelectedQueueTrack
        {
            get => _selectedQueueTrack;
            set => SetProperty(ref _selectedQueueTrack, value);
        }

        public RelayCommand EnqueueCommand { get; }
        public RelayCommand RemoveFromQueueCommand { get; }
        public RelayCommand ClearQueueCommand { get; }
        public RelayCommand PlayNowFromQueueCommand { get; }
        public RelayCommand PlayNextInQueueCommand { get; }

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

        public void Enqueue(TrackModel track)
        {
            if (track == null) return;

            var itemVm = new TrackItemViewModel(track, t =>
            {
                // Playing an item from the queue removes it and starts playback
                var found = QueueTracks.FirstOrDefault(q => q.Track.Id == t.Id);
                if (found != null)
                {
                    QueueTracks.Remove(found);
                }
                _onPlayTrack?.Invoke(t);
            });

            QueueTracks.Add(itemVm);
        }

        public TrackModel DequeueNext()
        {
            if (QueueTracks.Count == 0) return null;

            var first = QueueTracks[0];
            QueueTracks.RemoveAt(0);
            return first.Track;
        }

        public void Remove(TrackItemViewModel item)
        {
            if (item != null && QueueTracks.Contains(item))
            {
                QueueTracks.Remove(item);
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < QueueTracks.Count &&
                newIndex >= 0 && newIndex < QueueTracks.Count &&
                oldIndex != newIndex)
            {
                QueueTracks.Move(oldIndex, newIndex);
            }
        }

        public void Clear()
        {
            QueueTracks.Clear();
        }

        private void OnQueueTracksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateQueueStats();
        }

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

        // IDropTarget implementation for GongSolutions drag and drop
        public void DragOver(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            UpdateQueueStats();
        }
    }
}

