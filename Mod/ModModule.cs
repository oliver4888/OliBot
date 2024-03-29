﻿using DSharpPlus;
using OliBot.API.Attributes;
using OliBot.API.Interfaces;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        private async Task OnMessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Message.Content) &&
                e.Message.Attachments.Count == 0 &&
                e.Message.Embeds.Count == 0 &&
                e.Message.Flags.HasValue &&
                e.Message.Flags.Value.HasFlag(MessageFlags.SuppressedEmbeds))
            {
                _logger.LogDebug(
                    "Deleting blank message from {username}/{userId} in channel: {channelName}/{channelId}, guild: {guildName}/{guildId}",
                    e.Author.Username, e.Author.Id, e.Channel.Name, e.Channel.Id, e.Guild.Name, e.Guild.Id);
                await e.Message.DeleteAsync();
            }
        }
    }
}
