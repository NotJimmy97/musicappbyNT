using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SpotifyWpf.Bff.Services
{
    public class MemoryCacheService
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
        private static readonly object SyncRoot = new object();

        public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan slidingExpiration, Func<Task<T>> factory)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Fast path: Return cached object if still valid to avoid external network latency
            object existing = Cache.Get(key);
            if (existing is T typedExisting)
            {
                return typedExisting;
            }

            T created = await factory().ConfigureAwait(false);

            if (created != null)
            {
                var policy = new CacheItemPolicy
                {
                    SlidingExpiration = slidingExpiration
                };

                lock (SyncRoot)
                {
                    Cache.Set(key, created, policy);
                }
            }

            return created;
        }

        public void Set<T>(string key, T value, TimeSpan slidingExpiration)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return;
            }

            var policy = new CacheItemPolicy
            {
                SlidingExpiration = slidingExpiration
            };

            lock (SyncRoot)
            {
                Cache.Set(key, value, policy);
            }
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(T);
            }

            object value = Cache.Get(key);
            return value is T typed ? typed : default(T);
        }
    }
}
