using System;
using System.IO;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

namespace OliBot
{
    
    public class BotCore
    {
        static DiscordClient discord;
        public async Task Start()
        {
            string token = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt"));

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
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

            discord.SocketErrored += async e => {
                Console.WriteLine(e.Exception);
            };

            await discord.ConnectAsync(new DiscordActivity("v2 dev"));

            await Task.Delay(-1);
        }
    }
}
