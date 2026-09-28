using System.Threading.Tasks;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien quan ly bo nho dem luong am thanh truc tuyen (Spotify CAS Stream Cache Repository).
    /// </summary>
    public interface IStreamCacheRepository
    {
        Task<string> GetCachedFilePathAsync(string trackHash);
        Task RegisterCacheFileAsync(string trackHash, string filePath, long sizeBytes, bool isFull);
        Task TouchCacheAccessAsync(string trackHash);
        Task EvictOldestCacheAsync(long targetFreedBytes);
        Task<long> GetTotalCacheSizeAsync();
    }
}
