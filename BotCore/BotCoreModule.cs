using System;
using DSharpPlus;
using System.Linq;
using BotCore.Commands;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BotCore
{
    [Module(typeof(IBotCoreModule))]
    public class BotCoreModule : IBotCoreModule
    {
        readonly BotCore _config;
        readonly ILogger<BotCoreModule> _logger;

        public DiscordClient DiscordClient { get; private set; }
        public ICommandHandler CommandHandler { get; private set; }
        public DateTime StartTime { get; private set; }
        public ulong HostOwnerID => _config.HostOwnerID;

        public BotCoreModule(ILoggerFactory loggerFactory, BotCore config, IServiceProvider services)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger<BotCoreModule>();

            DiscordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.Token,
                TokenType = TokenType.Bot,
                LoggerFactory = loggerFactory
            });

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            DiscordClient.Ready += async e =>
            {
                _logger.LogInformation($"Ready in {e.Client.Guilds.Count} Guilds!");
            };

            DiscordClient.ClientErrored += async e =>
            {
                _logger.LogError(e.Exception, $"Error in {e.EventName} event");
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            CommandHandler = new CommandHandler(loggerFactory.CreateLogger<CommandHandler>(), this, services, _config.CommandPrefix);

            CommandHandler.RegisterCommands<CoreCommands>();
        }

        public async Task Start()
        {
            _logger.LogInformation($"Starting bot with {CommandHandler.Commands.Count()} commands: {string.Join(", ", CommandHandler.Commands.Select(command => command.Name))}");

            string status = _config.InitialStatus;

            await DiscordClient.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            StartTime = DateTime.Now;
        }
    }
}
