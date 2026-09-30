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
    /// ViewModel quản lý toàn bộ danh sách các playlist của người dùng.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Hiển thị danh sách Playlists, quản lý tạo mới/xóa/mở chi tiết.
    /// 2. Không chịu trách nhiệm: Thực thi query trực tiếp (ủy thác qua IPlaylistRepository).
    /// 3. Vòng đời: Tồn tại cùng MainViewModel.
    /// 4. Đa luồng: Các thao tác I/O DB chạy ngầm, Invoke qua Dispatcher để cập nhật ObservableCollection.
    /// State transitions:
    /// - Khi gọi OpenPlaylistCommand: Tạo PlaylistDetailViewModel, gán CurrentDetail và nạp Tracks.
    /// </remarks>
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

        public AsyncRelayCommand CreatePlaylistCommand { get; }
        public AsyncRelayCommand DeletePlaylistCommand { get; }
        public AsyncRelayCommand OpenPlaylistCommand { get; }
        public AsyncRelayCommand RefreshCommand { get; }

        public PlaylistsViewModel(IPlaylistRepository playlistRepo, ITrackRepository trackRepo, Action<TrackModel> onPlayTrack)
        {
            _playlistRepo = playlistRepo ?? throw new ArgumentNullException(nameof(playlistRepo));
            _trackRepo = trackRepo ?? throw new ArgumentNullException(nameof(trackRepo));
            _onPlayTrack = onPlayTrack;

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadPlaylistsAsync());

            OpenPlaylistCommand = new AsyncRelayCommand(async p =>
            {
                if (p is PlaylistEntity entity)
                {
                    await OpenPlaylistAsync(entity);
                }
            });

            DeletePlaylistCommand = new AsyncRelayCommand(async p =>
            {
                if (p is PlaylistEntity entity)
                {
                    await DeletePlaylistAsync(entity);
                }
            });

            CreatePlaylistCommand = new AsyncRelayCommand(async _ =>
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
