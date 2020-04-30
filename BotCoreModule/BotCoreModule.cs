using System;
using DSharpPlus;
using System.Linq;
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

        public DiscordClient DiscordClient { get; private set; }
        public ICommandHandler CommandHandler { get; private set; }
        public DateTime StartTime { get; private set; }

        public BotCoreModule(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _config = configuration.GetSection("BotCore");
            _logger = loggerFactory.CreateLogger<BotCoreModule>();

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

            CommandHandler = new CommandHandler(loggerFactory.CreateLogger<CommandHandler>(), this);

            CommandHandler.RegisterCommands<CoreCommands>();
        }

        public async Task Start()
        {
            _logger.LogInformation($"Starting bot with {CommandHandler.CommandNames.Count()} commands: {string.Join(", ", CommandHandler.CommandNames)}");

            string status = _config["InitialStatus"];

            await DiscordClient.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            StartTime = DateTime.Now;

            await Task.Delay(-1);
        }
    }
}
