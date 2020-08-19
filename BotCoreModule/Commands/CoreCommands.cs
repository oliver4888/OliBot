using Common;
using System;
using DSharpPlus;
using System.Text;
using Common.Attributes;
using Common.Extensions;
using Common.Interfaces;
using System.Reflection;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace BotCoreModule
{
    public class CoreCommands
    {
        readonly PropertyInfo _userCacheProperty = typeof(DiscordClient).GetProperty("UserCache", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        [Command]
        [Description("Returns some general stats on the bot.")]
        public async Task Stats(CommandContext ctx)
        {
            DiscordClient client = ctx.BotCoreModule.DiscordClient;
            var userCache = (ConcurrentDictionary<ulong, DiscordUser>)_userCacheProperty.GetValue(client);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle($"{client.CurrentUser.Username} Stats")
                .AddField("Server Count", client.Guilds.Count.ToString(), true)
                .AddField("Cached Users", userCache.Count.ToString(), true)
                .AddField("No. Commands", ctx.BotCoreModule.CommandHandler.Commands.Count.ToString(), true)
                .AddField("WS Ping", $"{client.Ping}ms", true)
                .AddField("DSharp+ Version", client.VersionString, true);

            if (!ctx.IsDMs)
                builder.WithCustomFooterWithColour(ctx);

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
            [Description("Get detailed help information for a particular command.")] string commandName = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                await ctx.Channel.SendMessageAsync(embed: GenHelpEmbed(ctx));
            else
            {
                if (!ctx.BotCoreModule.CommandHandler.TryGetCommand(commandName, out ICommand command))
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
            string botName = ctx.BotCoreModule.DiscordClient.CurrentUser.Username;

            string commandPrefix = ctx.BotCoreModule.CommandHandler.CommandPrefix;
            bool useCommandPrefix = !string.IsNullOrWhiteSpace(commandPrefix);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{botName} Help");

            string moreHelpMsg = $"{Environment.NewLine}Use `{(useCommandPrefix ? commandPrefix : $"@{botName} ")}help <command>` for help with a specific command.";

            if (ctx.IsDMs)
                embedBuilder.WithDescription($"Listing all available commands.{moreHelpMsg}");
            else
                embedBuilder
                    .WithDescription($"Listing all commands available to {ctx.Member.Mention}.{moreHelpMsg}")
                    .WithCustomFooterWithColour(ctx);

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
            string botName = ctx.BotCoreModule.DiscordClient.CurrentUser.Username;

            string commandPrefix = ctx.BotCoreModule.CommandHandler.CommandPrefix;
            bool useCommandPrefix = !string.IsNullOrWhiteSpace(commandPrefix);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{botName} Help: {command.Name}")
                .WithDescription(command.Description);

            if (!ctx.IsDMs)
                embedBuilder.WithCustomFooterWithColour(ctx);

            string usageText = $"{(useCommandPrefix ? commandPrefix : $"@{botName} ")}{command.Name}";
            embedBuilder.AddField("Usage", usageText);

            if (command.Triggers.Count > 1)
                embedBuilder.AddField("Aliases", $"`{string.Join("`, `", command.Triggers)}`");

            foreach (ICommandParameter param in command.Parameters)
            {
                if (param.Type == typeof(CommandContext))
                    continue;

                if (param.Required)
                    usageText += $" [{param.ParameterInfo.Name}]";
                else
                    usageText += $" ({param.ParameterInfo.Name}={param.ParameterInfo.DefaultValue})";

                embedBuilder.AddField(
                    param.ParameterInfo.Name,
                    $"**Description:** {param.Description}{Environment.NewLine}**Type:** `{param.Type.Name}{(param.Type.IsEnum ? " (enum)" : "")}`");
            }

            embedBuilder.Fields[0].Value = $"`{usageText}`";

            return embedBuilder.Build();
        }
    }
}
