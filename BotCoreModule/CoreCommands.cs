using Common;
using System;
using DSharpPlus;
using System.Text;
using Common.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace BotCoreModule
{
    public class CoreCommands
    {
        [Command]
        [Description("Ping pong?")]
        public async Task Ping(CommandContext ctx) =>
            await ctx.Message.Channel.SendMessageAsync("Pong!");

        [Command]
        [Description("Returns some general stats on the bot.")]
        public async Task Stats(CommandContext ctx)
        {
            DiscordClient client = ctx.BotCoreModule.DiscordClient;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle($"{client.CurrentUser.Username} Stats")
                .AddField("Server Count", client.Guilds.Count.ToString(), true)
                .AddField("Shard Count", client.ShardCount.ToString(), true)
                .AddField("WS Ping", $"{client.Ping}ms", true)
                .AddField("DSharp+ Version", client.VersionString, true);

            TimeSpan uptime = DateTime.Now - ctx.BotCoreModule.StartTime;
            StringBuilder uptimeBuilder = new StringBuilder();

            if (uptime.Days > 0)
                uptimeBuilder.Append(uptime.Days).Append(" Days ");

            uptimeBuilder.Append(uptime.ToString(@"hh\:mm\:ss"));

            builder.AddField("Uptime", uptimeBuilder.ToString(), true);

            await ctx.Message.Channel.SendMessageAsync(embed: builder.Build());
        }

        [Command(hidden: true, permissionLevel: BotPermissionLevel.HostOwner)]
        [Description("Only the host owner should be able to use this command")]
        public async Task TestHostOwner(CommandContext ctx) =>
            await ctx.Message.Channel.SendMessageAsync("Test Command");

        [Command(hidden: true, permissionLevel: BotPermissionLevel.Admin)]
        [Description("Only server admins should be able to use this command")]
        public async Task TestAdministrator(CommandContext ctx) =>
            await ctx.Message.Channel.SendMessageAsync("Test Command");
    }
}
