using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện quản lý các danh sách phát tùy biến của người dùng.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Quản lý CRUD (Create, Read, Update, Delete) danh sách phát.
    /// KHÔNG chịu trách nhiệm: Thực thi logic hiển thị UI.
    /// Vòng đời: Transient/Scoped tùy DI.
    /// Luồng/DB: Thao tác qua Task bất đồng bộ, lưu trữ ở local SQLite.
    /// </remarks>
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
