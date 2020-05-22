using Common;
using System;
using DSharpPlus;
using System.Text;
using System.Linq;
using Common.Attributes;
using Common.Extensions;
using Common.Interfaces;
using DSharpPlus.Entities;
using System.Threading.Tasks;

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

            foreach (ICommand command in ctx.BotCoreModule.CommandHandler.Commands)
            {
                if (command.Hidden ||
                    (command.PermissionLevel == BotPermissionLevel.HostOwner && ctx.Author.Id != ctx.BotCoreModule.HostOwnerID) ||
                    (command.PermissionLevel == BotPermissionLevel.Admin && !ctx.ChannelPermissions.HasFlag(Permissions.Administrator)))
                    continue;

                StringBuilder descriptionBuilder = new StringBuilder()
                    .AppendLine(command.Description)
                    .AppendLine($"**Usage:** {ctx.BotCoreModule.CommandHandler.CommandPrefix}{command.Name}");

                if (command.Triggers.Count > 1)
                    descriptionBuilder.AppendLine($"**Aliases:** `{string.Join("`, `", command.Triggers.Skip(1))}`");

                if (command.PermissionLevel == BotPermissionLevel.HostOwner)
                    descriptionBuilder.AppendLine("**Host Owner Only**");
                else if (command.PermissionLevel == BotPermissionLevel.Admin)
                    descriptionBuilder.AppendLine("**Admin Only**");

                embedBuilder.AddField(command.Name, descriptionBuilder.ToString());
            }

            await ctx.Channel.SendMessageAsync(embed: embedBuilder.Build());

            if (!ctx.IsDMs)
                await ctx.Message.DeleteAsync();
        }
    }
}
