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

                        if (!Regex.Match(word, @"^<@!?\d{18}>$").Success)
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
    }
}
