using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện thao tác dữ liệu kho bài hát hợp nhất.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Thao tác DB (CRUD) với thư viện nhạc hợp nhất (Local + Online).
    /// KHÔNG chịu trách nhiệm: Quản lý thư mục vật lý hay bóc tách ID3 Tag.
    /// Vòng đời: Transient/Scoped tùy DI.
    /// Luồng/DB: Thao tác bất đồng bộ qua Task. Sử dụng SQLite DB path cấu hình sẵn.
    /// </remarks>
    public interface ITrackRepository
    {
        Task<TrackEntity> GetByIdAsync(int id);
        Task<TrackEntity> GetByTrackKeyAsync(string trackKey);
        Task<TrackEntity> GetByFilePathAsync(string filePath);
        Task<IEnumerable<TrackEntity>> GetAllLocalTracksAsync();
        Task<IEnumerable<TrackEntity>> GetFavoritesAsync();
        Task<IEnumerable<TrackEntity>> SearchAsync(string keyword, int limit = 50);
        Task<IEnumerable<TrackEntity>> GetRecommendationsAsync(int limit = 25);
        Task<IEnumerable<TrackEntity>> GetSimilarTracksAsync(int trackId, int limit = 10);
        Task<int> InsertOrUpdateAsync(TrackEntity track);
        Task BatchInsertOrUpdateAsync(IEnumerable<TrackEntity> tracks);
        Task UpdateAffinityScoreAsync(int trackId, double deltaScore);
        Task ToggleFavoriteAsync(int trackId);
        Task IncrementPlayCountAsync(int trackId);
        Task IncrementSkipCountAsync(int trackId);
        Task DeleteTracksNotInFilesAsync(IEnumerable<string> existingFilePaths);
    }
}
