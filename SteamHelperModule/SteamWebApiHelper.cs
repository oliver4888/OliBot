using Steam.Models;
using System.Net.Http;
using System.Threading.Tasks;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using System.Collections.Generic;
using Steam.Models.SteamCommunity;
using Microsoft.Extensions.Logging;

namespace SteamHelperModule
{
    public class SteamWebApiHelper
    {
        readonly ILogger<SteamWebApiHelper> _logger;
        readonly HttpClient _client;

        const string PublishedFileCacheKey = "PublishedFileCache";
        const string PlayerSummaryCacheKey = "PlayerSummaryCache";

        readonly SteamWebInterfaceFactory SteamWebInterfaceFactory;
        readonly SteamRemoteStorage SteamRemoteStorage;
        readonly SteamUser SteamUser;

        static readonly IDictionary<string, SteamItemCache> _caches = new Dictionary<string, SteamItemCache>();
        public static IReadOnlyDictionary<string, SteamItemCache> Caches => _caches as IReadOnlyDictionary<string, SteamItemCache>;

        public SteamWebApiHelper(ILoggerFactory loggerFactory, string token, int absExpirationHourOffset)
        {
            _logger = loggerFactory.CreateLogger<SteamWebApiHelper>();
            _client = new HttpClient();

            // Caches of the same endpoint should be shared between instances
            if (!_caches.ContainsKey(PublishedFileCacheKey))
                _caches.Add(PublishedFileCacheKey, new SteamItemCache(loggerFactory.CreateLogger<SteamItemCache>(), PublishedFileCacheKey, absExpirationHourOffset));

            if (!_caches.ContainsKey(PlayerSummaryCacheKey))
                _caches.Add(PlayerSummaryCacheKey, new SteamItemCache(loggerFactory.CreateLogger<SteamItemCache>(), PlayerSummaryCacheKey, absExpirationHourOffset));

            SteamWebInterfaceFactory = new SteamWebInterfaceFactory(token);
            SteamRemoteStorage = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamRemoteStorage>(_client);
            SteamUser = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(_client);
        }

        public async Task<PublishedFileDetailsModel> GetPublishedFileDetails(ulong id) =>
            (await _caches[PublishedFileCacheKey].AddOrGetExisting(id.ToString(), async () => await SteamRemoteStorage.GetPublishedFileDetailsAsync(id))).Data;

        public async Task<PlayerSummaryModel> GetPlayerSummary(ulong id) =>
                (await _caches[PlayerSummaryCacheKey].AddOrGetExisting(id.ToString(), async () => await SteamUser.GetPlayerSummaryAsync(id))).Data;
    }
}
