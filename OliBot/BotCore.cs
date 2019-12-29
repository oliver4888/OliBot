using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;

namespace OliBot
{

    public class BotCore
    {
        static DiscordClient discord;
        static IConfigurationSection _config;

        public BotCore(IConfigurationRoot configuration) => _config = configuration.GetSection("BotCore");

        public async Task Start()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config["Token"],
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += async e =>
            {
                if (e.Author.IsBot) return;

                Console.WriteLine(e.Author.Username);

                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            discord.Ready += async e =>
            {
                Console.WriteLine("Ready!");
            };

            string status = _config["InitialStatus"];

            await discord.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            await Task.Delay(-1);
        }
    }
}
