using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel chi tiet mot danh sach phat (Playlist Detail ViewModel).
    /// Ho tro xem danh sach bai hat, xoa bai, doi thu tu va phat toan bo playlist.
    /// </summary>
    public class PlaylistDetailViewModel : ObservableObject
    {
        private readonly IPlaylistRepository _playlistRepo;
        private readonly Action<TrackModel> _onPlayTrack;
        private readonly Action _onNavigateBack;

        public PlaylistEntity Playlist { get; }
        public ObservableCollection<TrackItemViewModel> PlaylistTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        public string Name => Playlist.Name;
        public string Description => Playlist.Description;
        public int TracksCount => PlaylistTracks.Count;

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

        private TrackItemViewModel _selectedTrack;
        public TrackItemViewModel SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand BackCommand { get; }
        public RelayCommand PlayAllCommand { get; }
        public RelayCommand RemoveTrackCommand { get; }

        public PlaylistDetailViewModel(
            PlaylistEntity playlist,
            IPlaylistRepository playlistRepo,
            Action<TrackModel> onPlayTrack,
            Action onNavigateBack)
        {
            Playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
            _playlistRepo = playlistRepo ?? throw new ArgumentNullException(nameof(playlistRepo));
            _onPlayTrack = onPlayTrack;
            _onNavigateBack = onNavigateBack;

            BackCommand = new RelayCommand(_ => _onNavigateBack?.Invoke());

            PlayAllCommand = new RelayCommand(_ =>
            {
                if (PlaylistTracks.Count > 0)
                {
                    _onPlayTrack?.Invoke(PlaylistTracks[0].Track);
                }
            }, _ => PlaylistTracks.Count > 0);

            RemoveTrackCommand = new RelayCommand(async p =>
            {
                if (p is TrackItemViewModel itemVm)
                {
                    await RemoveTrackAsync(itemVm);
                }
            });
        }

        public async Task LoadTracksAsync()
        {
            IsLoading = true;
            try
            {
                var tracks = await _playlistRepo.GetTracksInPlaylistAsync(Playlist.Id).ConfigureAwait(false);
                var models = tracks.Select(e => new TrackModel
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

                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(() => PopulateTracks(models));
                }
                else
                {
                    PopulateTracks(models);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PopulateTracks(System.Collections.Generic.IList<TrackModel> tracks)
        {
            PlaylistTracks.Clear();
            int totalSec = 0;
            foreach (var t in tracks)
            {
                PlaylistTracks.Add(new TrackItemViewModel(t, _onPlayTrack));
                totalSec += t.DurationSeconds;
            }

            TotalDuration = TimeSpan.FromSeconds(totalSec);
            OnPropertyChanged(nameof(TracksCount));
            PlayAllCommand?.RaiseCanExecuteChanged();
        }

        public async Task RemoveTrackAsync(TrackItemViewModel itemVm)
        {
            if (itemVm == null) return;
            if (int.TryParse(itemVm.Track.Id, out int trackId))
            {
                await _playlistRepo.RemoveTrackFromPlaylistAsync(Playlist.Id, trackId).ConfigureAwait(false);
            }

            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                {
                    PlaylistTracks.Remove(itemVm);
                    OnPropertyChanged(nameof(TracksCount));
                    PlayAllCommand?.RaiseCanExecuteChanged();
                });
            }
            else
            {
                PlaylistTracks.Remove(itemVm);
                OnPropertyChanged(nameof(TracksCount));
                PlayAllCommand?.RaiseCanExecuteChanged();
            }
        }
    }
}
