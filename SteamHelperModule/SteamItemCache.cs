using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamHelperModule
{
    public class SteamItemCache
    {
        readonly ILogger<SteamItemCache> _logger;
        readonly MemoryCache _cache;
        readonly int _absExpirationHourOffset;

        readonly object _lockObject = new object();

        public long CacheItemCount => _cache.GetCount();

        public SteamItemCache(ILogger<SteamItemCache> logger, string cacheName, int absExpirationHourOffset)
        {
            _logger = logger;
            _cache = new MemoryCache(cacheName);
            _absExpirationHourOffset = absExpirationHourOffset;
        }

        public Task<T> AddOrGetExisting<T>(string key, Func<Task<T>> valueFactory)
        {
            lock (_lockObject)
            {
                var newValue = new Lazy<Task<T>>(valueFactory);
                var oldValue = _cache.AddOrGetExisting(key, newValue, new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddHours(_absExpirationHourOffset) }) as Lazy<Task<T>>;
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
