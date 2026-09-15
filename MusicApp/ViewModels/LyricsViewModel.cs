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
    public class LyricsViewModel : ObservableObject
    {
        private readonly ILyricsService _lyricsService;
        private readonly Action<TimeSpan> _onSeek;
        private CancellationTokenSource _loadCts;

        public ObservableCollection<LyricLineViewModel> Lines { get; } = new ObservableCollection<LyricLineViewModel>();

        private TrackModel _currentTrack;
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

        public string CurrentSongTitle => CurrentTrack?.Title ?? "Chưa chọn bài hát";
        public string CurrentSongArtist => CurrentTrack?.Artist ?? "Chọn một bài hát để xem lời đồng bộ";

        private bool _hasLyrics;
        public bool HasLyrics
        {
            get => _hasLyrics;
            private set => SetProperty(ref _hasLyrics, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        private int _activeLineIndex = -1;
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

        public LyricLineViewModel ActiveLine => (_activeLineIndex >= 0 && _activeLineIndex < Lines.Count) ? Lines[_activeLineIndex] : null;

        public event EventHandler<int> ActiveLineChanged;

        public LyricsViewModel(ILyricsService lyricsService, Action<TimeSpan> onSeek)
        {
            _lyricsService = lyricsService ?? throw new ArgumentNullException(nameof(lyricsService));
            _onSeek = onSeek;
        }

        public async Task LoadLyricsForTrackAsync(TrackModel track)
        {
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

        public void UpdatePosition(TimeSpan position)
        {
            if (Lines.Count == 0) return;

            // Binary search for greatest index i where Lines[i].Timestamp <= position
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

            // Only update visual tree when the line actually changes (state transition gate)
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
