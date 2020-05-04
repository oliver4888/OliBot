using DSharpPlus;
using Common.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Generic;

namespace Common
{
    public class CommandContext
    {
        private readonly MessageCreateEventArgs _event;

        public readonly IBotCoreModule BotCoreModule;

        public DiscordMessage Message => _event.Message;
        public DiscordChannel Channel => _event.Channel;
        public DiscordGuild Guild => _event.Guild;
        public DiscordUser Author => _event.Author;
        public DiscordMember Member { get; private set; }

        public Permissions ChannelPermissions { get; private set; }

        public IReadOnlyList<DiscordUser> MentionedUsers => _event.MentionedUsers;
        public IReadOnlyList<DiscordRole> MentionedRoles => _event.MentionedRoles;
        public IReadOnlyList<DiscordChannel> MentionedChannels => _event.MentionedChannels;

        public CommandContext(MessageCreateEventArgs messageCreateEventArgs, IBotCoreModule botCoreModule, DiscordMember discordMember, Permissions channelPermissions)
        {
            _event = messageCreateEventArgs;

            BotCoreModule = botCoreModule;
            Member = discordMember;

            ChannelPermissions = channelPermissions;
        }
    }
}
