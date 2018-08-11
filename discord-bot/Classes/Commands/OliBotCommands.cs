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

            if (
                ctx.User.Id != OliBotCore.Oliver4888Id &&
                !ctx.Guild.GetMemberAsync(ctx.User.Id).Result.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages))
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, You are not authorized to use this command!");
                return;
            }

            if (limit > 100)
                limit = 100;

            if (channel == null)
                channel = ctx.Channel;

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
    }
}
