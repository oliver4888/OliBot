using Common;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BotCoreModule
{
    [Module]
    public class BotCoreModule
    {
        IConfigurationSection _config;
        ILogger<BotCoreModule> _logger;

        public static DiscordClient discordClient;

        public BotCoreModule(ILogger<BotCoreModule> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration.GetSection("BotCore");
        }

        public async Task Start()
        {
            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _config["Token"],
                TokenType = TokenType.Bot
            });

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            discordClient.Ready += async e =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                _logger.LogInformation($"Ready in {e.Client.Guilds.Count} Guilds!");
            };

            string status = _config["InitialStatus"];

            await discordClient.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            await Task.Delay(-1);
        }
    }
}
