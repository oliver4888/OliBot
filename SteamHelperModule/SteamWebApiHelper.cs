using Steam.Models;
using System.Net.Http;
using System.Threading.Tasks;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using Steam.Models.SteamCommunity;
using Microsoft.Extensions.Logging;

namespace SteamHelperModule
{
    public class SteamWebApiHelper
    {
        readonly ILogger<SteamWebApiHelper> _logger;
        readonly HttpClient _client;

        readonly SteamItemCache _publishedFileCache;
        readonly SteamItemCache _playerSummaryCache;

        readonly SteamWebInterfaceFactory SteamWebInterfaceFactory;
        readonly SteamRemoteStorage SteamRemoteStorage;
        readonly SteamUser SteamUser;

        public SteamWebApiHelper(ILoggerFactory loggerFactory, string token)
        {
            _logger = loggerFactory.CreateLogger<SteamWebApiHelper>();
            _client = new HttpClient();

            _publishedFileCache = new SteamItemCache(loggerFactory.CreateLogger<SteamItemCache>(), "PublishedFileCache");
            _playerSummaryCache = new SteamItemCache(loggerFactory.CreateLogger<SteamItemCache>(), "PlayerSummaryCache");

            SteamWebInterfaceFactory = new SteamWebInterfaceFactory(token);
            SteamRemoteStorage = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamRemoteStorage>(_client);
            SteamUser = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(_client);
        }

        public async Task<PublishedFileDetailsModel> GetPublishedFileDetails(ulong id)
        {
            ISteamWebResponse<PublishedFileDetailsModel> response = await _publishedFileCache.AddOrGetExisting(id.ToString(), async () => await SteamRemoteStorage.GetPublishedFileDetailsAsync(id));
            return response.Data;
        }

        public async Task<PlayerSummaryModel> GetPlayerSummary(ulong id)
        {
            ISteamWebResponse<PlayerSummaryModel> response = await _playerSummaryCache.AddOrGetExisting(id.ToString(), async () => await SteamUser.GetPlayerSummaryAsync(id));
            return response.Data;
        }
    }
}
