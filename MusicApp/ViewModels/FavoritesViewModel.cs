using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quan ly danh sach bai hat yeu thich cua nguoi dung (Favorites ViewModel).
    /// Nap truc tiep tu SQLite database voi toc do cao va ho tro tim kiem, loc va phat nhac.
    /// </summary>
    public class FavoritesViewModel : ObservableObject
    {
        private readonly ITrackRepository _trackRepo;
        private readonly Action<TrackModel> _onPlayTrack;
        private readonly List<TrackModel> _allFavorites = new List<TrackModel>();

        public ObservableCollection<TrackItemViewModel> FavoriteTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        private int _totalTracksCount;
        public int TotalTracksCount
        {
            get => _totalTracksCount;
            private set => SetProperty(ref _totalTracksCount, value);
        }

        private TimeSpan _totalDuration = TimeSpan.Zero;
        public TimeSpan TotalDuration
        {
            get => _totalDuration;
            private set
            {
                if (SetProperty(ref _totalDuration, value))
                {
                    OnPropertyChanged(nameof(FormattedTotalDuration));
                }
            }
        }

        public string FormattedTotalDuration
        {
            get
            {
                if (TotalDuration.TotalHours >= 1)
                {
                    return $"{(int)TotalDuration.TotalHours} giờ {TotalDuration.Minutes} phút";
                }
                return $"{TotalDuration.Minutes} phút {TotalDuration.Seconds} giây";
            }
        }

        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                if (SetProperty(ref _searchFilter, value))
                {
                    ApplyFilter();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private TrackItemViewModel _selectedTrack;
        public TrackItemViewModel SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        public RelayCommand PlayAllCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand RemoveFavoriteCommand { get; }

        public FavoritesViewModel(ITrackRepository trackRepo, Action<TrackModel> onPlayTrack)
        {
            _trackRepo = trackRepo ?? throw new ArgumentNullException(nameof(trackRepo));
            _onPlayTrack = onPlayTrack;

            PlayAllCommand = new RelayCommand(_ =>
            {
                if (FavoriteTracks.Count > 0)
                {
                    _onPlayTrack?.Invoke(FavoriteTracks[0].Track);
                }
            }, _ => FavoriteTracks.Count > 0);

            RefreshCommand = new RelayCommand(async _ => await LoadFavoritesAsync());

            RemoveFavoriteCommand = new RelayCommand(async p =>
            {
                if (p is TrackItemViewModel itemVm)
                {
                    await RemoveFavoriteAsync(itemVm);
                }
            });
        }

        public async Task LoadFavoritesAsync()
        {
            IsLoading = true;
            try
            {
                var entities = await _trackRepo.GetFavoritesAsync().ConfigureAwait(false);
                var models = entities.Select(e => new TrackModel
                {
                    Id = e.Id.ToString(),
                    Title = e.Title,
                    Artist = e.Artist,
                    Album = e.Album,
                    DurationSeconds = e.DurationSeconds,
                    CoverImageUrl = e.CoverUri,
                    StreamUrl = e.SourceType == "local" ? e.SourceId : e.SourceId,
                    Genre = e.Genre
                }).ToList();

                _allFavorites.Clear();
                _allFavorites.AddRange(models);

                // Update on UI Thread
                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(() => UpdateCollectionAndStats(models));
                }
                else
                {
                    UpdateCollectionAndStats(models);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateCollectionAndStats(IList<TrackModel> tracks)
        {
            FavoriteTracks.Clear();
            int totalSec = 0;
            foreach (var t in tracks)
            {
                FavoriteTracks.Add(new TrackItemViewModel(t, _onPlayTrack));
                totalSec += t.DurationSeconds;
            }

            TotalTracksCount = FavoriteTracks.Count;
            TotalDuration = TimeSpan.FromSeconds(totalSec);
            PlayAllCommand?.RaiseCanExecuteChanged();
        }

        private void ApplyFilter()
        {
            string kw = SearchFilter?.Trim().ToLowerInvariant() ?? string.Empty;
            FavoriteTracks.Clear();

            var filtered = string.IsNullOrEmpty(kw)
                ? _allFavorites
                : _allFavorites.Where(t =>
                    (t.Title != null && t.Title.ToLowerInvariant().Contains(kw)) ||
                    (t.Artist != null && t.Artist.ToLowerInvariant().Contains(kw)) ||
                    (t.Album != null && t.Album.ToLowerInvariant().Contains(kw)));

            int totalSec = 0;
            foreach (var t in filtered)
            {
                FavoriteTracks.Add(new TrackItemViewModel(t, _onPlayTrack));
                totalSec += t.DurationSeconds;
            }

            TotalTracksCount = FavoriteTracks.Count;
            TotalDuration = TimeSpan.FromSeconds(totalSec);
            PlayAllCommand?.RaiseCanExecuteChanged();
        }

        private async Task RemoveFavoriteAsync(TrackItemViewModel itemVm)
        {
            if (itemVm == null) return;
            if (int.TryParse(itemVm.Track.Id, out int trackId))
            {
                await _trackRepo.ToggleFavoriteAsync(trackId).ConfigureAwait(false);
            }

            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                {
                    _allFavorites.RemoveAll(t => t.Id == itemVm.Track.Id);
                    FavoriteTracks.Remove(itemVm);
                    TotalTracksCount = FavoriteTracks.Count;
                    PlayAllCommand?.RaiseCanExecuteChanged();
                });
            }
            else
            {
                _allFavorites.RemoveAll(t => t.Id == itemVm.Track.Id);
                FavoriteTracks.Remove(itemVm);
                TotalTracksCount = FavoriteTracks.Count;
                PlayAllCommand?.RaiseCanExecuteChanged();
            }
        }
    }
}
