using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamHelperModule
{
    public class SteamItemCache
    {
        readonly ILogger<SteamItemCache> _logger;
        readonly MemoryCache _cache;
        readonly CacheItemPolicy _cachePolicy;
        readonly string _fileCachePath;

        static readonly Mutex _mutex = new Mutex();

        public long CacheItemCount => _cache.GetCount();

        public SteamItemCache(ILogger<SteamItemCache> logger, string cacheName, int slidingExpirationHours, string fileCachePath, Type objectType)
        {
            _logger = logger;
            _cache = new MemoryCache(cacheName);

            _cachePolicy = new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromHours(slidingExpirationHours),
                RemovedCallback = e => DeleteCacheFile(e.CacheItem.Key)
            };

            _fileCachePath = Path.Combine(Environment.CurrentDirectory, fileCachePath, cacheName);

            LoadFileCachedItems(objectType);
        }

        private void LoadFileCachedItems(Type objectType)
        {
            if (!Directory.Exists(_fileCachePath))
                Directory.CreateDirectory(_fileCachePath);
            else if (Directory.EnumerateFiles(_fileCachePath).Any())
            {
                Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(Directory.EnumerateFiles(_fileCachePath), async fileName =>
                    {
                        try
                        {
                            using FileStream fs = File.OpenRead(fileName);
                            object data = await JsonSerializer.DeserializeAsync(fs, objectType);
                            string key = Path.GetFileNameWithoutExtension(fileName);
                            _cache.Add(key, data, _cachePolicy);
                            _logger.LogDebug($"Loaded cache item from file for {_cache.Name}/{key}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Unable to load cache item from file: ${fileName}");
                        }
                    });
                });
            }
        }

        public static SteamItemCache Create<T>(ILogger<SteamItemCache> logger, string cacheName, int slidingExpirationHours, string fileCachePath) =>
            new SteamItemCache(logger, cacheName, slidingExpirationHours, fileCachePath, typeof(T));

        public async Task<T> AddOrGetExisting<T>(string key, Func<Task<T>> valueFactory)
        {
            try
            {
                _mutex.WaitOne();

                if (_cache.Contains(key))
                {
                    return (T)_cache.Get(key);
                }
                else
                {
                    T data = await valueFactory();
                    _cache.Add(key, data, _cachePolicy);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            using FileStream fs = File.Create(Path.Combine(_fileCachePath, key + ".json"));
                            await JsonSerializer.SerializeAsync(fs, data);
                            _logger.LogDebug($"Created cache file for {_cache.Name}/{key}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Unable to create cache file for {_cache.Name}/{key}");
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching item from cache {_cache.Name} key {key}");
                _cache.Remove(key);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            return default;
        }

        public void DeleteCacheFile(string key)
        {
            try
            {
                string file = Path.Combine(_fileCachePath, key + ".json");
                if (File.Exists(file))
                {
                    File.Delete(file);
                    _logger.LogDebug($"Removed cache file for {_cache.Name}/{key}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to delete cache file for {_cache.Name}/{key}");
            }
        }

        public void Clear()
        {
            foreach (string key in _cache.Select(kvp => kvp.Key))
                RemoveItem(key);
        }

        public void RemoveItem(string key) => _cache.Remove(key);
    }
}
