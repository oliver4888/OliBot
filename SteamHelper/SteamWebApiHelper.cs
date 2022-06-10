using OliBot.API;
using Steam.Models;
using System.Net.Http;
using OliBot.API.Attributes;
using SteamHelper.Models;
using System.Threading.Tasks;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using System.Collections.Generic;
using Steam.Models.SteamCommunity;
using Microsoft.Extensions.Logging;

namespace SteamHelper
{
    public interface ISteamWebApiHelper
    {
        public IReadOnlyDictionary<string, SteamItemCache> Caches { get; }

        public Task<PublishedFileDetailsModel> GetPublishedFileDetails(ulong id);
        public Task<PlayerSummaryModel> GetPlayerSummary(ulong id);
        public Task<SteamAppDetails> GetStoreDetails(uint id);
    }

    [DependencyInjected(DIType.Singleton, typeof(ISteamWebApiHelper))]
    public class SteamWebApiHelper : ISteamWebApiHelper
    {
        readonly ILogger<SteamWebApiHelper> _logger;
        readonly HttpClient _client;

        const string PublishedFileCacheKey = "PublishedFileCache";
        const string PlayerSummaryCacheKey = "PlayerSummaryCache";
        const string StoreAppDetailsCacheKey = "StoreAppDetailsCache";

        readonly SteamWebInterfaceFactory SteamWebInterfaceFactory;
        readonly SteamRemoteStorage SteamRemoteStorage;
        readonly SteamUser SteamUser;
        readonly SteamStore SteamStore;

        readonly IDictionary<string, SteamItemCache> _caches = new Dictionary<string, SteamItemCache>();
        public IReadOnlyDictionary<string, SteamItemCache> Caches => _caches as IReadOnlyDictionary<string, SteamItemCache>;

        public SteamWebApiHelper(ILoggerFactory loggerFactory, SteamHelper config)
        {
            _logger = loggerFactory.CreateLogger<SteamWebApiHelper>();
            _client = new HttpClient();

            _caches.Add(
                PublishedFileCacheKey,
                SteamItemCache.Create<PublishedFileDetailsModel>(
                    loggerFactory.CreateLogger<SteamItemCache>(),
                    PublishedFileCacheKey,
                    config.SlidingExpirationHours,
                    config.FileCachePath));

            _caches.Add(
                PlayerSummaryCacheKey,
                SteamItemCache.Create<PlayerSummaryModel>(
                    loggerFactory.CreateLogger<SteamItemCache>(),
                    PlayerSummaryCacheKey,
                    config.SlidingExpirationHours,
                    config.FileCachePath));

            _caches.Add(
                StoreAppDetailsCacheKey,
                SteamItemCache.Create<SteamAppDetails>(
                    loggerFactory.CreateLogger<SteamItemCache>(),
                    StoreAppDetailsCacheKey,
                    config.SlidingExpirationHours,
                    config.FileCachePath));

            SteamWebInterfaceFactory = new SteamWebInterfaceFactory(config.Token);
            SteamRemoteStorage = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamRemoteStorage>(_client);
            SteamUser = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(_client);
            SteamStore = SteamWebInterfaceFactory.CreateSteamStoreInterface(_client);
        }

        public async Task<PublishedFileDetailsModel> GetPublishedFileDetails(ulong id) =>
            await _caches[PublishedFileCacheKey].AddOrGetExisting(
                id.ToString(),
                async () => (await SteamRemoteStorage.GetPublishedFileDetailsAsync(id))?.Data,
                data => data != null && data.Result != 9 && data.Visibility == PublishedFileVisibility.Public);

        public async Task<PlayerSummaryModel> GetPlayerSummary(ulong id) =>
                await _caches[PlayerSummaryCacheKey].AddOrGetExisting(id.ToString(), async () => (await SteamUser.GetPlayerSummaryAsync(id))?.Data);

        public async Task<SteamAppDetails> GetStoreDetails(uint id) =>
            await _caches[StoreAppDetailsCacheKey].AddOrGetExisting(id.ToString(), async () => (SteamAppDetails)await SteamStore.GetStoreAppDetailsAsync(id, "gb"));
    }
}
