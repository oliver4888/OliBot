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
                .AddField("No. Commands", ctx.BotCoreModule.CommandHandler.Commands.Count.ToString(), true)
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
        public async Task Help(CommandContext ctx,
            [Description("Get detailed help information for a particular command.")]string commandName = null)
        {
            if (commandName == null)
                await ctx.Channel.SendMessageAsync(embed: GenHelpEmbed(ctx));
            else
            {
                ICommand command = ctx.BotCoreModule.CommandHandler.Commands.FirstOrDefault(c => c.Triggers.Contains(commandName));
                if (command == null)
                {
                    await ctx.Channel.SendMessageAsync($"{ctx.Author.Mention}, unable to find command `{commandName}`!");
                    return;
                }
                else
                {
                    if (HasPermissions(ctx, command))
                        await ctx.Channel.SendMessageAsync(embed: GenHelpEmbedForCommand(ctx, command));
                    else
                        return;
                }
            }

            if (!ctx.IsDMs)
                await ctx.Message.DeleteAsync();
        }

        private bool HasPermissions(CommandContext ctx, ICommand command) => !(command.Hidden || (ctx.IsDMs && command.DisableDMs) ||
                    (command.PermissionLevel == BotPermissionLevel.HostOwner && ctx.Author.Id != ctx.BotCoreModule.HostOwnerID) ||
                    (command.PermissionLevel == BotPermissionLevel.Admin && !ctx.ChannelPermissions.HasFlag(Permissions.Administrator)));

        private DiscordEmbed GenHelpEmbed(CommandContext ctx)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.BotCoreModule.DiscordClient.CurrentUser.Username} Help");

            if (ctx.IsDMs)
                embedBuilder.WithDescription("Listing all available commands.");
            else
                embedBuilder
                    .WithDescription($"Listing all commands available to {ctx.Member.Mention}.")
                    .WithCustomFooterWithColour(ctx.Message, ctx.Member);

            IDictionary<string, IList<string>> commandGroups = new Dictionary<string, IList<string>>();

            foreach (ICommand command in ctx.BotCoreModule.CommandHandler.Commands)
            {
                if (!HasPermissions(ctx, command))
                    continue;

                if (!commandGroups.ContainsKey(command.GroupName))
                    commandGroups.Add(command.GroupName, new List<string>());

                commandGroups[command.GroupName].Add(command.Name);
            }

            foreach ((string group, IList<string> commands) in commandGroups)
                embedBuilder.AddField(group, $"`{string.Join("`, `", commands)}`", true);

            return embedBuilder.Build();
        }

        private DiscordEmbed GenHelpEmbedForCommand(CommandContext ctx, ICommand command)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"Help: {command.Name}")
                .WithDescription(command.Description);

            if (!ctx.IsDMs)
                embedBuilder.WithCustomFooterWithColour(ctx.Message, ctx.Member);

            if (command.Triggers.Count > 1)
                embedBuilder.AddField("Aliases", $"`{string.Join("`, `", command.Triggers)}`");

            foreach (ICommandParameter param in command.Parameters)
            {
                if (param.Type == typeof(CommandContext))
                    continue;

                string description = $"**Description:** {param.Description}{Environment.NewLine}**Type:** `{param.Type.Name}`";

                if (!param.Required)
                    description += $"{Environment.NewLine}**Default Value:** {param.ParameterInfo.DefaultValue}";

                embedBuilder.AddField(param.ParameterInfo.Name, description);
            }

            return embedBuilder.Build();
        }
    }
}
