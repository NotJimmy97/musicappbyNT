using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace MusicApp.Bff.Services
{
    /// <summary>
    /// Dich vu luu dem trong bo nho RAM cua tien trinh (In-Memory Cache Service).
    /// 
    /// Tac dung:
    /// - Luu tru tam thoi ket qua tim kiem bai hat hoac thong tin metadata trong bo nho RAM.
    /// - Cung cap phuong thuc GetOrCreateAsync su dung mau Cache-Aside Pattern.
    /// - Ho tro co che het han truot (Sliding Expiration): Reset lai thoi gian song neu item tiep tuc duoc truy cap.
    /// 
    /// Van de giai quyet:
    /// - Giam thieu so luong yeu cau mang ra ngoai Internet den Jamendo API hoac CDN Archive.org khi nguoi dung tim kiem lai tu khoa quen thuoc.
    /// - Tranh tinh trang bi gioi han tan suat goi (Rate Limiting) tu cac may chu am nhac ben thu ba.
    /// - Dam bao an toan dong thoi (Thread-Safe) khi nhieu luong cung yeu cau doc/ghi mot khoa bo dem bang doi tuong SyncLock.
    /// 
    /// Cach thuc van hanh:
    /// - Kiem tra MemoryCache.Default voi khoa key. Neu co san gia tri dung kieu T, tra ve ngay lap tuc ma khong goi factory.
    /// - Neu chua co, thuc thi ham delegate factory() bat dong bo, luu ket qua vao cache roi tra ve cho caller.
    /// </summary>
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
