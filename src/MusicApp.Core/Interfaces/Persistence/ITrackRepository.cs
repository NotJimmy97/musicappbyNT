using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien thao tac du lieu kho bai hat hop nhat (Unified Track Catalog Repository).
    /// </summary>
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
