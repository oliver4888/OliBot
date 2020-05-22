using Common;
using System;
using DSharpPlus;
using System.Text;
using Common.Attributes;
using Common.Extensions;
using Common.Interfaces;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BotCoreModule
{
    public class CoreCommands
    {
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

            if (!ctx.IsDMs)
                builder.WithCustomFooterWithColour(ctx.Message, ctx.Member);

            TimeSpan uptime = DateTime.Now - ctx.BotCoreModule.StartTime;
            StringBuilder uptimeBuilder = new StringBuilder();

            if (uptime.Days > 0)
                uptimeBuilder.Append(uptime.Days).Append($" Day{(uptime.Days == 1 ? "" : "s")} ");

            uptimeBuilder.Append(uptime.ToString(@"hh\:mm\:ss"));

            builder.AddField("Uptime", uptimeBuilder.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed: builder.Build());

            if (!ctx.IsDMs)
                await ctx.Message.DeleteAsync();
        }

        [Command]
        [Description("Displays help information for commands available to the user that run this command.")]
        public async Task Help(CommandContext ctx)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.BotCoreModule.DiscordClient.CurrentUser.Username} Help");

            if (ctx.IsDMs)
                embedBuilder.WithDescription("Listing all available commands.");
            else
                embedBuilder
                    .WithDescription($"Listing all commands available to {ctx.Member.Mention}.") // Specify a command to see more information using: ??help <command>") // TODO
                    .WithCustomFooterWithColour(ctx.Message, ctx.Member);

            IDictionary<string, IList<string>> commandGroups = new Dictionary<string, IList<string>>();

            foreach (ICommand command in ctx.BotCoreModule.CommandHandler.Commands)
            {
                if (command.Hidden || (ctx.IsDMs && command.DisableDMs) ||
                    (command.PermissionLevel == BotPermissionLevel.HostOwner && ctx.Author.Id != ctx.BotCoreModule.HostOwnerID) ||
                    (command.PermissionLevel == BotPermissionLevel.Admin && !ctx.ChannelPermissions.HasFlag(Permissions.Administrator)))
                    continue;

                if (!commandGroups.ContainsKey(command.GroupName))
                    commandGroups.Add(command.GroupName, new List<string>());

                commandGroups[command.GroupName].Add(command.Name);
            }

            foreach ((string group, IList<string> commands) in commandGroups)
                embedBuilder.AddField(group, $"`{string.Join("`, `", commands)}`");

            await ctx.Channel.SendMessageAsync(embed: embedBuilder.Build());

            if (!ctx.IsDMs)
                await ctx.Message.DeleteAsync();
        }
    }
}
