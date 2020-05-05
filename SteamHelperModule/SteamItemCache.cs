using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamHelperModule
{
    public class SteamItemCache
    {
        ILogger<SteamItemCache> _logger;
        readonly MemoryCache _cache;

        object _lockObject = new object();

        public SteamItemCache(ILogger<SteamItemCache> logger, string cacheName)
        {
            _logger = logger;
            _cache = new MemoryCache(cacheName);
        }

        public Task<T> AddOrGetExisting<T>(string key, Func<Task<T>> valueFactory)
        {
            lock (_lockObject)
            {
                var newValue = new Lazy<Task<T>>(valueFactory);
                var oldValue = _cache.AddOrGetExisting(key, newValue, new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddHours(6) }) as Lazy<Task<T>>;
                try
                {
                    return (oldValue ?? newValue).Value;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fetching item from cache {_cache.Name} key {key}");
                    _cache.Remove(key);
                    return default;
                }
            }
        }
    }
}
