using Common;
using Steam.Models;
using System.Net.Http;
using Common.Attributes;
using SteamHelper.Models;
using System.Threading.Tasks;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using System.Collections.Generic;
using Steam.Models.SteamCommunity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SteamHelper
{
    public interface ISteamWebApiHelper
    {
        public static IReadOnlyDictionary<string, SteamItemCache> Caches;

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

        static readonly IDictionary<string, SteamItemCache> _caches = new Dictionary<string, SteamItemCache>();
        public static IReadOnlyDictionary<string, SteamItemCache> Caches => _caches as IReadOnlyDictionary<string, SteamItemCache>;

        public SteamWebApiHelper(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<SteamWebApiHelper>();
            _client = new HttpClient();

            IConfigurationSection config = configuration.GetSection("SteamHelper");

            int slidingExpirationHours = int.Parse(config["SlidingExpirationHours"]);
            string fileCachePath = config["FileCachePath"];

            // Caches of the same endpoint should be shared between instances
            if (!_caches.ContainsKey(PublishedFileCacheKey))
                _caches.Add(
                    PublishedFileCacheKey,
                    SteamItemCache.Create<PublishedFileDetailsModel>(
                        loggerFactory.CreateLogger<SteamItemCache>(),
                        PublishedFileCacheKey,
                        slidingExpirationHours,
                        fileCachePath));

            if (!_caches.ContainsKey(PlayerSummaryCacheKey))
                _caches.Add(
                    PlayerSummaryCacheKey,
                    SteamItemCache.Create<PlayerSummaryModel>(
                        loggerFactory.CreateLogger<SteamItemCache>(),
                        PlayerSummaryCacheKey,
                        slidingExpirationHours,
                        fileCachePath));

            if (!_caches.ContainsKey(StoreAppDetailsCacheKey))
                _caches.Add(
                    StoreAppDetailsCacheKey,
                    SteamItemCache.Create<SteamAppDetails>(
                        loggerFactory.CreateLogger<SteamItemCache>(),
                        StoreAppDetailsCacheKey,
                        slidingExpirationHours,
                        fileCachePath));

            SteamWebInterfaceFactory = new SteamWebInterfaceFactory(config["Token"]);
            SteamRemoteStorage = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamRemoteStorage>(_client);
            SteamUser = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(_client);
            SteamStore = SteamWebInterfaceFactory.CreateSteamStoreInterface(_client);
        }

        public async Task<PublishedFileDetailsModel> GetPublishedFileDetails(ulong id) =>
            await _caches[PublishedFileCacheKey].AddOrGetExisting(id.ToString(), async () => (await SteamRemoteStorage.GetPublishedFileDetailsAsync(id))?.Data);

        public async Task<PlayerSummaryModel> GetPlayerSummary(ulong id) =>
                await _caches[PlayerSummaryCacheKey].AddOrGetExisting(id.ToString(), async () => (await SteamUser.GetPlayerSummaryAsync(id))?.Data);

        public async Task<SteamAppDetails> GetStoreDetails(uint id) =>
            await _caches[StoreAppDetailsCacheKey].AddOrGetExisting(id.ToString(), async () => (SteamAppDetails)await SteamStore.GetStoreAppDetailsAsync(id));
    }
}
