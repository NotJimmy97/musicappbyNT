using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

// OWNS: Logic bộ nhớ đệm tạm thời (RAM cache) cho metadata kết quả tìm kiếm.
// DOES NOT OWN: Cache file âm thanh thực tế trên đĩa (disk cache).
// CONSTRAINTS: Phải đảm bảo an toàn luồng (Thread-safe) do có truy xuất đồng thời (concurrent access).

namespace MusicApp.Bff.Services
{
    /// <summary>
    /// Dịch vụ lưu đệm trong bộ nhớ RAM của tiến trình (In-Memory Cache).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Cung cấp API lưu trữ tạm `GetOrCreateAsync` cho các kết quả API/Router tốn thời gian.
    /// 2. Không chịu trách nhiệm: Persistence dài hạn, không thay thế cho Database.
    /// 3. Vòng đời trạng thái: Cache chỉ là tạm thời (RAM only), mất hoàn toàn khi tiến trình BFF bị restart.
    /// 4. Yêu cầu đặc biệt: Các thao tác đọc/ghi sử dụng SyncLock để đảm bảo luồng (Thread-safe).
    /// </remarks>
    public class MemoryCacheService
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
        private static readonly object SyncLock = new object();

        /// <summary>
        /// Lay gia tri tu bo nho dem neu da ton tai, hoac tao moi gia tri bang ham factory va luu vao cache.
        /// </summary>
        /// <typeparam name="T">Kieu du lieu can luu tru trong cache.</typeparam>
        /// <param name="key">Khoa dinh danh duy nhat cua ban ghi cache.</param>
        /// <param name="slidingExpiration">Thoi gian het han truot ke tu lan truy cap cuoi cung.</param>
        /// <param name="factory">Delegate bat dong bo de tao moi gia tri neu cache bi bo lo (Cache Miss).</param>
        /// <returns>Gia tri duoc lay tu cache hoac duoc tao moi.</returns>
        public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan slidingExpiration, Func<Task<T>> factory)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            // 1. Kiem tra xem ban ghi da co san trong bo nho cache chua
            object cached = Cache.Get(key);
            if (cached is T typedValue)
            {
                return typedValue;
            }

            // 2. Neu chua co, thuc thi ham factory de lay du lieu goc tu nguon
            T created = await factory().ConfigureAwait(false);
            if (created != null)
            {
                lock (SyncLock)
                {
                    var policy = new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    };
                    Cache.Set(key, created, policy);
                }
            }

            return created;
        }
    }
}
