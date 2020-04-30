using System;
using DSharpPlus;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BotCoreModule
{
    [Module]
    public class BotCoreModule : IBotCoreModule
    {
        readonly IConfigurationSection _config;
        readonly ILogger<BotCoreModule> _logger;

        public static DiscordClient DiscordClient { get; private set; }
        public static ICommandHandler CommandHandler { get; private set; }
        public static DateTime StartTime { get; private set; }

        public BotCoreModule(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _config = configuration.GetSection("BotCore");
            _logger = loggerFactory.CreateLogger<BotCoreModule>();
            CommandHandler = new CommandHandler(loggerFactory.CreateLogger<CommandHandler>(), this);

            CommandHandler.RegisterCommands<CoreCommands>();
        }

        public async Task Start()
        {
            DiscordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _config["Token"],
                TokenType = TokenType.Bot
            });

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            DiscordClient.Ready += async e =>
            {
                _logger.LogInformation($"Ready in {e.Client.Guilds.Count} Guilds!");
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            string status = _config["InitialStatus"];

            await DiscordClient.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            StartTime = DateTime.Now;

            await Task.Delay(-1);
        }
    }
}
