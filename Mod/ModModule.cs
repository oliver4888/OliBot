using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus;

namespace Mod
{
    [Module]
    public class ModModule
    {
        readonly ILogger<ModModule> _logger;
        readonly IBotCoreModule _botCoreModule;

        public ModModule(ILogger<ModModule> logger, IBotCoreModule botCoreModule)
        {
            _logger = logger;
            _botCoreModule = botCoreModule;

            _botCoreModule.CommandHandler.RegisterCommands<ModCommands>();
            _botCoreModule.DiscordClient.MessageUpdated += OnMessageUpdated;
        }

        private async Task OnMessageUpdated(MessageUpdateEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Message.Content) &&
                e.Message.Attachments.Count == 0 &&
                e.Message.Embeds.Count == 0 &&
                e.Message.Flags.HasValue &&
                e.Message.Flags.Value.HasFlag(MessageFlags.SuppressedEmbeds))
            {
                _logger.LogDebug($"Deleting blank message from {e.Author.Username}({e.Author.Id}) in channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}");
                await e.Message.DeleteAsync();
            }
        }
    }
}
