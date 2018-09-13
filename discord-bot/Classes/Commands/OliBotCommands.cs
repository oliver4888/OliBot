using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discord_bot;
using discord_bot.Classes;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Text.RegularExpressions;

namespace discord_bot.Classes
{
    public class OliBotCommands
    {
        // I don't know if there is an easier way to avoid bots and only reply in dev channel when debugging ¯\_(ツ)_/¯

        [Command("src")]
        [Description("Get yourself a link to OliBot's source code!")]
        [Aliases("source", "source-code", "sourcecode")]
        public async Task Src(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            await ctx.RespondAsync($"{ctx.User.Mention}, the OliBot source code can be found at: https://github.com/oliver4888/OliBot");
        }

        [Command("ping")]
        [Description("ping pong!")]
        public async Task Ping(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#endif
            ) return;

            await ctx.RespondAsync($"{ctx.User.Mention} pong!");
        }

        [Command("strats")]
        [Description("Get a link to the strats Google sheet")]
        public async Task Strats(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            if (ctx.Guild.Id != OliBotCore.CSGOStratGuildId)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, this command isn't available in {ctx.Guild.Name}");
                return;
            }

            await ctx.RespondAsync($"{ctx.User.Mention}, request access on this sheet to add strats https://docs.google.com/spreadsheets/d/18E2BtjDeYESqs6Zo6tELthR20aDaBFlYRx4weXCFSOE/edit#gid=0");
        }

        [Command("strat")]
        [Description("Get a random strat")]
        public async Task Strat(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            if (ctx.Guild.Id != OliBotCore.CSGOStratGuildId)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, this command isn't available in {ctx.Guild.Name}");
                return;
            }

            await ctx.RespondAsync($"{ctx.User.Mention}: {StratHelper.GetRandomStrat()}");
        }

        [Command("update-strats")]
        [Description("Updates OliBot's cache of the strats list from the Google sheet")]
        [Aliases("updatestrats", "strats-update", "stratsupdate")]
        public async Task UpdateStrats(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            if (ctx.Guild.Id != OliBotCore.CSGOStratGuildId)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, this command isn't available in {ctx.Guild.Name}");
                return;
            }

            await ctx.RespondAsync($"{ctx.User.Mention}: {await StratHelper.UpdateStrats()}");
        }

        [Command("clear")]
        [Description("Clears messages from the current channel")]
        public async Task Clear(
            CommandContext ctx,
            [Description("Number of messages to clear, max 100")]int limit = 100
            )
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (!(member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages) || member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator)))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            if (limit > 100) limit = 100;

            await ctx.Message.DeleteAsync();

            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(limit));
            DiscordMessage message = await ctx.RespondAsync($"Deleted {limit.ToString()} message{(limit == 1 ? "" : "s")} :white_check_mark:");
            await Task.Delay(5000);
            await message.DeleteAsync();
        }

        [Command("clear-mentions")]
        [Aliases("clearm")]
        [Description("Clears messages from the current channel that are only @ mentions")]
        public async Task ClearMentions(
            CommandContext ctx,
            [Description("Number of messages to search through, max 100")]int limit = 100
            )
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (!(member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages) || member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator)))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            if (limit > 100) limit = 100;

            await ctx.Message.DeleteAsync();

            IReadOnlyList<DiscordMessage> discordMessages = await ctx.Channel.GetMessagesAsync(limit);
            List<DiscordMessage> messagesToDelete = new List<DiscordMessage>();

            Parallel.ForEach(discordMessages, message =>
            {
                string[] words = message.Content.Split(new char[] { ' ', ',', '.', ':', '\t' });

                bool isOnlyMentions = true;

                foreach (string word in words)
                {
                    if (!Regex.Match(word, @"<@!?\d{18}>").Success)
                    {
                        isOnlyMentions = false;
                        break;
                    }
                }

                if (isOnlyMentions)
                {
                    messagesToDelete.Add(message);
                }
            });

            await ctx.Channel.DeleteMessagesAsync(messagesToDelete);

            DiscordMessage response = await ctx.RespondAsync($"Deleted {messagesToDelete.Count.ToString()} message{(messagesToDelete.Count == 1 ? "" : "s")} :white_check_mark:");
            await Task.Delay(5000);
            await response.DeleteAsync();
        }

        [Command("remove-recent-reactions")]
        [Description("Removes all reactions from the specified channel for the past 100 messages")]
        [Aliases("rrr")]
        public async Task RemoveRecentReactions(
            CommandContext ctx,
            [Description("Which channel OliBot will remove reactions from")] DiscordChannel channel = null,
            [Description("Whose reactions OliBot will remove")] DiscordUser user = null,
            [Description("How many messages should OliBot remove messages from, max 100")] int limit = 100,
            [RemainingText] [Description("Why are the reactions being removed")] string reason = null)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (!(member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages) || member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator)))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            if (limit > 100) limit = 100;

            if (channel == null) channel = ctx.Channel;

            int reactionsRemoved = 0;
            int fromMessages = 0;

            IReadOnlyList<DiscordMessage> messages = await channel.GetMessagesAsync(limit);

            Parallel.ForEach(messages, message =>
            {
                if (message.Reactions.Count > 0)
                {
                    fromMessages++;
                    if (user == null)
                    {
                        reactionsRemoved += message.Reactions.Count;
                        message.DeleteAllReactionsAsync(reason);
                    }
                    else
                    {
                        foreach (DiscordReaction reaction in message.Reactions)
                        {
                            reactionsRemoved++;
                            message.DeleteReactionAsync(reaction.Emoji, user, reason);
                        }
                    }
                }
            });

            if (reactionsRemoved == 0 && fromMessages == 0)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, I couldn't find any reactions to remove ");
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, Removed {reactionsRemoved} reaction{(reactionsRemoved > 0 ? "s" : "")} from {fromMessages} message{(fromMessages > 0 ? "s" : "")}!");
            }
        }

        [Command("check-mute")]
        [Hidden]
        [Aliases("cm")]
        [Description("Ensures that the Muted role has Permission.SendMessages DENIED in every text channel for the current guild")]
        public async Task CheckMute(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            if (ctx.User.Id != OliBotCore.Oliver4888Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            DiscordRole muted = await OliBotCore.Instance.GetMutedRole(ctx.Guild);

            foreach (DiscordChannel channel in ctx.Guild.Channels)
            {
                if (channel.Type != ChannelType.Text)
                    continue;

                await channel.AddOverwriteAsync(muted, Permissions.None, Permissions.SendMessages);
            }
            await ctx.RespondAsync($"{ctx.User.Mention}, done!");
        }

        [Command("mute")]
        public async Task Mute(
            CommandContext ctx,
            [Description("The user to mute")] DiscordUser user,
            [RemainingText] [Description("Reason for the mute")] string reason = ""
            )
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (!(member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles) || member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator)))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            DiscordRole muted = await OliBotCore.Instance.GetMutedRole(ctx.Guild);
            await ctx.Guild.GrantRoleAsync(ctx.Guild.GetMemberAsync(user.Id).Result, muted, reason);

            await ctx.RespondAsync($"{user.Mention} was muted for \"{(reason == "" ? "unspecified" : reason)}\" by {ctx.User.Mention}");
        }

        [Command("unmute")]
        public async Task UnMute(
            CommandContext ctx,
            [Description("The user to unmute")] DiscordUser user,
            [RemainingText] [Description("Reason for the unmute")] string reason = ""
            )
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (!(member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles) || member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator)))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            DiscordRole muted = await OliBotCore.Instance.GetMutedRole(ctx.Guild);
            await ctx.Guild.RevokeRoleAsync(ctx.Guild.GetMemberAsync(user.Id).Result, muted, reason);

            await ctx.RespondAsync($"{user.Mention} was unmuted for \"{(reason == "" ? "unspecified" : reason)}\" by {ctx.User.Mention}");
        }

        [Command("add-status")]
        [Aliases("+status")]
        [Description("Adds a status that the bot can use")]
        public async Task AddStatus(
            CommandContext ctx,
            [RemainingText] [Description("A status to add")] string status)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            if (ctx.User.Id != OliBotCore.Oliver4888Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            if (OliBotCore.Instance.AddStatus(status))
            {
                string message = $"{ctx.User.Mention}, added status \"{status}\"!";
                OliBotCore.Log.Info(message);
                await ctx.RespondAsync(message);
                return;
            }
            await ctx.RespondAsync($"{ctx.User.Mention}, something went wrong D:");
        }

        [Command("set-status")]
        [Description("Sets the bot's status")]
        [Aliases("status")]
        public async Task SetStatus(
    CommandContext ctx,
    [RemainingText] [Description("Set the bots status to this")] string status)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            if (ctx.User.Id != OliBotCore.Oliver4888Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            await OliBotCore.Instance.SetStatus(status);
            OliBotCore.Instance.StatusTimer.Reset();
        }

        [Command("wow")]
        [Description("Wow")]
        public async Task Wow(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            await ctx.RespondWithFileAsync($"{AppDomain.CurrentDomain.BaseDirectory}/content/images/wow.jpg");
        }

        [Command("doit")]
        [Description("Just do it!")]
        public async Task DoIt(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            await ctx.RespondWithFileAsync($"{AppDomain.CurrentDomain.BaseDirectory}/content/images/doit.gif");
        }

        [Command("pong")]
        [Description("ping pong!")]
        public async Task Pong(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            await ctx.RespondWithFileAsync($"{AppDomain.CurrentDomain.BaseDirectory}/content/images/stop-get-help.gif");
        }

        [Command("get-help")]
        [Aliases("stop-it", "stopit", "gethelp")]
        [Description("Get some help!")]
        public async Task StopGetHelp(CommandContext ctx)
        {
            if (ctx.User.IsBot
#if DEBUG == false
                || ctx.Channel.Id == OliBotCore.DevChannelId
#else
                || ctx.Channel.Id != OliBotCore.DevChannelId
#endif
            ) return;

            await ctx.RespondWithFileAsync($"{AppDomain.CurrentDomain.BaseDirectory}/content/images/stop-get-help.gif");
        }
    }
}
