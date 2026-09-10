using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace MusicApp.Bff.Services
{
    public class MemoryCacheService
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
        private static readonly object SyncLock = new object();

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

            object cached = Cache.Get(key);
            if (cached is T typedValue)
            {
                return typedValue;
            }

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

