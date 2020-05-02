using Common;
using System;
using DSharpPlus;
using System.Text;
using Common.Attributes;
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
                .WithTimestamp(ctx.Message.Id)
                .WithColor(ctx.DiscordMember.Color)
                .WithFooter($"{ctx.DiscordMember.Username} used {ctx.Message.Content.Split(' ')[0]}", ctx.DiscordMember.AvatarUrl)
                .AddField("Server Count", client.Guilds.Count.ToString(), true)
                .AddField("Shard Count", client.ShardCount.ToString(), true)
                .AddField("WS Ping", $"{client.Ping}ms", true)
                .AddField("DSharp+ Version", client.VersionString, true);

            TimeSpan uptime = DateTime.Now - ctx.BotCoreModule.StartTime;
            StringBuilder uptimeBuilder = new StringBuilder();

            if (uptime.Days > 0)
                uptimeBuilder.Append(uptime.Days).Append($" Day{(uptime.Days == 1 ? "" : "s")} ");

            uptimeBuilder.Append(uptime.ToString(@"hh\:mm\:ss"));

            builder.AddField("Uptime", uptimeBuilder.ToString(), true);

            await ctx.Message.DeleteAsync();
            await ctx.Message.Channel.SendMessageAsync(embed: builder.Build());
        }

        [Command]
        [Description("Displays help information for commands available to the user that run this command.")]
        public async Task Help(CommandContext ctx)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.BotCoreModule.DiscordClient.CurrentUser.Username} Help")
                .WithDescription($"Listing all commands available to {ctx.DiscordMember.Mention}.") // Specify a command to see more information using: ??help <command>") // TODO
                .WithTimestamp(ctx.Message.Id)
                .WithColor(ctx.DiscordMember.Color)
                .WithFooter($"{ctx.DiscordMember.Username} used {ctx.Message.Content.Split(' ')[0]}", ctx.DiscordMember.AvatarUrl);

            Permissions channelPermissions = ctx.Message.Channel.PermissionsFor(ctx.DiscordMember);

            foreach (ICommand command in ctx.BotCoreModule.CommandHandler.Commands)
            {
                if (command.Hidden ||
                    (command.PermissionLevel == BotPermissionLevel.HostOwner && ctx.DiscordMember.Id != ctx.BotCoreModule.HostOwnerID) ||
                    (command.PermissionLevel == BotPermissionLevel.Admin && !channelPermissions.HasFlag(Permissions.Administrator)))
                    continue;

                StringBuilder descriptionBuilder = new StringBuilder()
                    .AppendLine(command.Description)
                    .AppendLine($"**Usage:** ??{command.Name}");

                if (command.PermissionLevel == BotPermissionLevel.HostOwner)
                    descriptionBuilder.AppendLine("**Host Owner Only**");
                else if (command.PermissionLevel == BotPermissionLevel.Admin)
                    descriptionBuilder.AppendLine("**Admin Only**");

                embedBuilder.AddField(command.Name, descriptionBuilder.ToString());
            }

            await ctx.Message.DeleteAsync();
            await ctx.Message.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
