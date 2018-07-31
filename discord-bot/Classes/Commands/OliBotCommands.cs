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

            await ctx.RespondAsync($"{ctx.User.Mention}, the OliBot source code can be found at: https://gitlab.com/Oliver4888/discord-bot");
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
    }
}
