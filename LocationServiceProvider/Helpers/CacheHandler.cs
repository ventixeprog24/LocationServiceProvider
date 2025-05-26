using LocationServiceProvider.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LocationServiceProvider.Helpers
{
    public class CacheHandler(IMemoryCache cache) : ICacheHandler
    {
        private readonly IMemoryCache _cache = cache;
        public IEnumerable<Location>? GetFromCache(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out IEnumerable<Location>? cachedData))
                return cachedData;

            return default;
        }

        public IEnumerable<Location> SetCache(string cacheKey, IEnumerable<Location> data, int minutesToCache = 10)
        {
            _cache.Remove(cacheKey);
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(minutesToCache));
            return data;
        }
    }
}
