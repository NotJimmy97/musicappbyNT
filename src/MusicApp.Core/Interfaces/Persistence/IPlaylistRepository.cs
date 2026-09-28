using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien quan ly cac danh sach phat tuy bien cua nguoi dung (Custom Playlists Repository).
    /// </summary>
    public interface IPlaylistRepository
    {
        Task<IEnumerable<PlaylistEntity>> GetAllPlaylistsAsync();
        Task<PlaylistEntity> GetByIdAsync(int playlistId);
        Task<PlaylistEntity> CreatePlaylistAsync(string name, string description = null);
        Task UpdatePlaylistAsync(int playlistId, string name, string description);
        Task DeletePlaylistAsync(int playlistId);
        Task AddTrackToPlaylistAsync(int playlistId, int trackId);
        Task RemoveTrackFromPlaylistAsync(int playlistId, int trackId);
        Task ReorderTracksAsync(int playlistId, IEnumerable<int> orderedTrackIds);
        Task<IEnumerable<TrackEntity>> GetTracksInPlaylistAsync(int playlistId);
    }
}
