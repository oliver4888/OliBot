using System.Linq;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace SteamHelperModule
{
    [Module]
    public class SteamHelperModule
    {
        readonly ILogger<SteamHelperModule> _logger;
        readonly IConfigurationSection _config;
        readonly IBotCoreModule _botCoreModule;

        const string SteamClientLinkAffix = "steam://url/CommunityFilePage/";
        const string _regexString = @"(http(s)?:\/\/)?steam(community\.com\/sharedfiles\/filedetails\/\?id=|:\/\/url\/CommunityFilePage\/)(\d{9,10})";
        readonly Regex _steamRegex = new Regex(_regexString, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public SteamHelperModule(ILogger<SteamHelperModule> logger, IConfiguration configuration, IBotCoreModule botCoreModule)
        {
            _logger = logger;
            _config = configuration.GetSection("SteamHelper");
            _botCoreModule = botCoreModule;

            _botCoreModule.DiscordClient.MessageCreated += OnMessageCreated;
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || string.IsNullOrWhiteSpace(e.Message.Content)) return;

            MatchCollection matches = _steamRegex.Matches(e.Message.Content);

            if (!matches.Any()) return;

            //if (matches.Count() == 1 && e.Message.Content.Trim() == matches[0].Value)
            //    await e.Message.DeleteAsync();

            //await e.Channel.SendMessageAsync(SteamClientLinkAffix + matches[0].Groups.Last().Value);
        }
    }
}
