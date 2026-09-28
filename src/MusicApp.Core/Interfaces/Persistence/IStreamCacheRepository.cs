using System.Threading.Tasks;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện quản lý bộ nhớ đệm luồng âm thanh trực tuyến.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ, đánh dấu file đệm (Cache) trên đĩa để tối ưu stream.
    /// KHÔNG chịu trách nhiệm: Tải stream từ mạng.
    /// Vòng đời: Transient/Scoped tùy DI.
    /// Luồng/DB: Thao tác I/O bất đồng bộ an toàn luồng (Thread-safe) do có thể truy cập đồng thời.
    /// </remarks>
    public interface IStreamCacheRepository
    {
        Task<string> GetCachedFilePathAsync(string trackHash);
        Task RegisterCacheFileAsync(string trackHash, string filePath, long sizeBytes, bool isFull);
        Task TouchCacheAccessAsync(string trackHash);
        Task EvictOldestCacheAsync(long targetFreedBytes);
        Task<long> GetTotalCacheSizeAsync();
    }
}
