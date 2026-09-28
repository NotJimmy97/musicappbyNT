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
    /// ViewModel quan ly toan bo danh sach cac playlist cua nguoi dung (Playlists Overview ViewModel).
    /// Cho phep xem danh sach dang Card/Bento, tao playlist moi, xoa playlist va mo xem chi tiet.
    /// </summary>
    public class PlaylistsViewModel : ObservableObject
    {
        private readonly IPlaylistRepository _playlistRepo;
        private readonly ITrackRepository _trackRepo;
        private readonly Action<TrackModel> _onPlayTrack;

        public ObservableCollection<PlaylistEntity> Playlists { get; } = new ObservableCollection<PlaylistEntity>();

        private PlaylistEntity _selectedPlaylist;
        public PlaylistEntity SelectedPlaylist
        {
            get => _selectedPlaylist;
            set => SetProperty(ref _selectedPlaylist, value);
        }

        private PlaylistDetailViewModel _currentDetail;
        public PlaylistDetailViewModel CurrentDetail
        {
            get => _currentDetail;
            set
            {
                if (SetProperty(ref _currentDetail, value))
                {
                    OnPropertyChanged(nameof(IsInDetailMode));
                }
            }
        }

        public bool IsInDetailMode => CurrentDetail != null;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand CreatePlaylistCommand { get; }
        public RelayCommand DeletePlaylistCommand { get; }
        public RelayCommand OpenPlaylistCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public PlaylistsViewModel(IPlaylistRepository playlistRepo, ITrackRepository trackRepo, Action<TrackModel> onPlayTrack)
        {
            _playlistRepo = playlistRepo ?? throw new ArgumentNullException(nameof(playlistRepo));
            _trackRepo = trackRepo ?? throw new ArgumentNullException(nameof(trackRepo));
            _onPlayTrack = onPlayTrack;

            RefreshCommand = new RelayCommand(async _ => await LoadPlaylistsAsync());

            OpenPlaylistCommand = new RelayCommand(async p =>
            {
                if (p is PlaylistEntity entity)
                {
                    await OpenPlaylistAsync(entity);
                }
            });

            DeletePlaylistCommand = new RelayCommand(async p =>
            {
                if (p is PlaylistEntity entity)
                {
                    await DeletePlaylistAsync(entity);
                }
            });

            CreatePlaylistCommand = new RelayCommand(async _ =>
            {
                // Prompt UI dialog for name
                var dialog = new MusicApp.Views.CreatePlaylistDialog();
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.PlaylistName))
                {
                    await CreatePlaylistAsync(dialog.PlaylistName, dialog.PlaylistDescription);
                }
            });
        }

        public async Task LoadPlaylistsAsync()
        {
            IsLoading = true;
            try
            {
                var list = await _playlistRepo.GetAllPlaylistsAsync().ConfigureAwait(false);
                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(() =>
                    {
                        Playlists.Clear();
                        foreach (var p in list)
                        {
                            Playlists.Add(p);
                        }
                    });
                }
                else
                {
                    Playlists.Clear();
                    foreach (var p in list)
                    {
                        Playlists.Add(p);
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OpenPlaylistAsync(PlaylistEntity playlist)
        {
            if (playlist == null) return;

            var detailVm = new PlaylistDetailViewModel(
                playlist,
                _playlistRepo,
                _onPlayTrack,
                onNavigateBack: () => CurrentDetail = null);

            CurrentDetail = detailVm;
            await detailVm.LoadTracksAsync();
        }

        public async Task CreatePlaylistAsync(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            var created = await _playlistRepo.CreatePlaylistAsync(name, description).ConfigureAwait(false);
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() => Playlists.Add(created));
            }
            else
            {
                Playlists.Add(created);
            }
        }

        public async Task DeletePlaylistAsync(PlaylistEntity playlist)
        {
            if (playlist == null) return;

            await _playlistRepo.DeletePlaylistAsync(playlist.Id).ConfigureAwait(false);
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                {
                    Playlists.Remove(playlist);
                    if (CurrentDetail != null && CurrentDetail.Playlist.Id == playlist.Id)
                    {
                        CurrentDetail = null;
                    }
                });
            }
            else
            {
                Playlists.Remove(playlist);
                if (CurrentDetail != null && CurrentDetail.Playlist.Id == playlist.Id)
                {
                    CurrentDetail = null;
                }
            }
        }
    }
}
