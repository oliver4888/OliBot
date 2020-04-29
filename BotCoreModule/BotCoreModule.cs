using Common;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BotCoreModule
{
    [Module]
    public class BotCoreModule
    {
        IConfigurationSection _config;

        public static DiscordClient discordClient;

        public BotCoreModule(IConfigurationRoot configuration) => _config = configuration.GetSection("BotCore");

        public async Task Start()
        {
            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _config["Token"],
                TokenType = TokenType.Bot
            });

            string status = _config["InitialStatus"];

            await discordClient.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            await Task.Delay(-1);
        }
    }
}
