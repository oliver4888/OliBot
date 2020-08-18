using Common;
using System;
using DSharpPlus;
using System.Linq;
using Common.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ModModule
{
    public class ModCommands
    {
        [Command(disableDMs: true, groupName: "Moderation")]
        [RequiredPermissions(Permissions.ManageMessages)]
        [Description("Clears the specified number of messages. Max 100.")]
        public async Task Clear(CommandContext ctx, int numMessages = 0)
        {
            numMessages = Math.Clamp(numMessages, 0, 100);

            if (numMessages == 0)
            {
                await ctx.Channel.SendMessageAsync("Please specify a number of messages to remove. Max 100.");
                return;
            }

            await ctx.Message.DeleteAsync();

            IReadOnlyList<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync(numMessages);
            await ctx.Channel.DeleteMessagesAsync(messages);
            DiscordMessage response = await ctx.Channel.SendMessageAsync($"Deleted {messages.Count} message{(messages.Count == 1 ? "" : "s")} :white_check_mark:");

            await DelayThenDelete(ctx, response);
        }

        [Command(disableDMs: true, groupName: "Moderation")]
        [Alias("clearm")]
        [RequiredPermissions(Permissions.ManageMessages)]
        [Description("Clears messages from the current channel that are only @ mentions.")]
        public async Task ClearMentions(CommandContext ctx, int numMessages = 100)
        {
            numMessages = Math.Clamp(numMessages, 0, 100);

            if (numMessages == 0)
            {
                await ctx.Channel.SendMessageAsync("Please specify a number of messages to clear mentions from. Max 100.");
                return;
            }

            IReadOnlyList<DiscordMessage> discordMessages = await ctx.Channel.GetMessagesAsync(numMessages);
            IList<DiscordMessage> messagesToDelete = new List<DiscordMessage>();

            char[] wordSplitters = new char[] { ' ', ',', '.', ':', '\t' };

            Parallel.ForEach(discordMessages, message =>
            {
                if (message.Attachments.Count == 0 && message.Embeds.Count == 0)
                {
                    string[] words = message.Content.Split(wordSplitters);

                    bool isOnlyMentions = true;

                    foreach (string word in words)
                    {
                        if (string.IsNullOrWhiteSpace(word) || word == "@everyone" || word == "@here") break;

                        if (!Regex.Match(word, @"^<@[!&]?\d{18}>$").Success)
                        {
                            isOnlyMentions = false;
                            break;
                        }
                    }

                    if (isOnlyMentions)
                        messagesToDelete.Add(message);
                }
            });

            DiscordMessage response;

            if (messagesToDelete.Any())
            {
                await ctx.Channel.DeleteMessagesAsync(messagesToDelete);

                response = await ctx.Channel.SendMessageAsync($"Deleted {messagesToDelete.Count} message{(messagesToDelete.Count == 1 ? "" : "s")} :white_check_mark:");
            }
            else
                response = await ctx.Channel.SendMessageAsync($"No mention only messages where found in the past {numMessages} messages!");

            await DelayThenDelete(ctx, response);
        }


        [Command(disableDMs: true, groupName: "Moderation")]
        [Alias("cm", "check-mute")]
        [RequiredPermissions(Permissions.ManageChannels)]
        [Description("Ensures that the Muted role has Permission.SendMessages DENIED in every text channel for the current guild.")]
        public async Task CheckMute(CommandContext ctx)
        {
            DiscordRole muted = await GetMutedRole(ctx.Guild);

            foreach (DiscordChannel channel in ctx.Guild.Channels.Values)
            {
                if (channel.Type != ChannelType.Text)
                    continue;

                await channel.AddOverwriteAsync(muted, deny: Permissions.SendMessages);
            }

            await ctx.Message.RespondAsync($"{ctx.Member.Mention}, done!");
        }

        [Command(disableDMs: true, groupName: "Moderation")]
        [RequiredPermissions(Permissions.ManageRoles)]
        public async Task Mute(CommandContext ctx,
            [Description("The user to mute")] DiscordMember member,
            [RemainingText] [Description("Reason for the mute")] string reason = "")
        {
            DiscordRole muted = await GetMutedRole(ctx.Guild);
            await member.GrantRoleAsync(muted, reason);

            await ctx.Message.RespondAsync($"{member.Mention} was muted for \"{(reason == "" ? "unspecified" : reason)}\" by {ctx.Member.Mention}");
        }

        [Command(disableDMs: true, groupName: "Moderation")]
        [RequiredPermissions(Permissions.ManageRoles)]
        public async Task UnMute(CommandContext ctx,
            [Description("The user to unmute")] DiscordMember member,
            [RemainingText] [Description("Reason for the unmute")] string reason = "")
        {
            DiscordRole muted = await GetMutedRole(ctx.Guild);
            await member.RevokeRoleAsync(muted, reason);

            await ctx.Message.RespondAsync($"{member.Mention} was unmuted for \"{(reason == "" ? "unspecified" : reason)}\" by {ctx.Member.Mention}");
        }

        [Command(disableDMs: true, groupName: "Moderation")]
        [RequiredPermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx,
            [Description("User to kick")] DiscordMember member,
            [RemainingText] [Description("Reason for the kick")] string reason = "")
        {
            try
            {
                await member.RemoveAsync(reason);
                await ctx.Message.RespondAsync("Goodbye! :wave: They're gone.");
            }
            catch (Exception)
            {
                await ctx.Message.RespondAsync("Ah :poop: something went wrong.");
            }
        }

        #region Helper Methods
        private async Task<DiscordRole> GetMutedRole(DiscordGuild guild) =>
            guild.Roles.Values.FirstOrDefault(role => role.Name.ToLowerInvariant() == "muted") ?? await guild.CreateRoleAsync("Muted", mentionable: true);

        private async Task DelayThenDelete(CommandContext ctx, DiscordMessage response = null, int delay = 2500)
        {
            await Task.Delay(delay);
            try
            {
                await ctx.Message.DeleteAsync();
            }
            catch (Exception) { }
            try
            {
                if (response != null)
                    await response.DeleteAsync();
            }
            catch (Exception) { }
        }
        #endregion
    }
}
