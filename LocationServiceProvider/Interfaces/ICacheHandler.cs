namespace LocationServiceProvider.Interfaces
{
    public interface ICacheHandler
    {
        IEnumerable<Location>? GetFromCache(string cacheKey);
        IEnumerable<Location> SetCache(string cacheKey, IEnumerable<Location> data, int minutesToCache = 10);
    }
}
