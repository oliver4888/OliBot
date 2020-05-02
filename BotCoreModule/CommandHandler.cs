using Common;
using System;
using DSharpPlus;
using System.Linq;
using System.Reflection;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace BotCoreModule
{
    public class CommandHandler : ICommandHandler
    {
        // This will be moved to a db and configurable per server 
        const string _commandPrefix = "??";

        readonly ILogger<CommandHandler> _logger;
        readonly IBotCoreModule _botCoreModuleInstance;

        readonly IDictionary<string, CommandListingValue> _commands = new Dictionary<string, CommandListingValue>();
        public IReadOnlyDictionary<string, CommandListingValue> Commands => _commands as IReadOnlyDictionary<string, CommandListingValue>;

        public CommandHandler(ILogger<CommandHandler> logger, IBotCoreModule botCoreModuleInstance)
        {
            _logger = logger;
            _botCoreModuleInstance = botCoreModuleInstance;
            _botCoreModuleInstance.DiscordClient.MessageCreated += OnMessageCreated;
        }

        public void RegisterCommands<T>() => RegisterCommands(typeof(T));

        public void RegisterCommands(Type commandClass)
        {
            if (commandClass == null)
                throw new ArgumentNullException(nameof(commandClass));

            IEnumerable<MethodInfo> commands = commandClass.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(CommandAttribute), false));

            if (!commands.Any())
            {
                _logger.LogWarning($"No commands where found in the given type: {commandClass.Name}");
                return;
            }

            object commandClassInstance = Activator.CreateInstance(commandClass);

            foreach (MethodInfo command in commands)
            {
                CommandAttribute ca = command.GetCustomAttribute<CommandAttribute>();
                _commands.Add(
                    ca.CommandName == "" ? command.Name.ToLowerInvariant() : ca.CommandName,
                    new CommandListingValue(commandClass, commandClassInstance, command));
            }

            _logger.LogInformation($"Registered {_commands.Count()} command(s) for Type {commandClass.FullName}");
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !e.Message.Content.StartsWith(_commandPrefix)) return;

            string[] messageParts = e.Message.Content.Remove(0, _commandPrefix.Length).Split(" ");

            string command = messageParts[0].ToLowerInvariant();

            if (_commands.ContainsKey(command))
                await HandleCommand(e, command, messageParts);
        }

        private async Task HandleCommand(MessageCreateEventArgs e, string command, string[] messageParts)
        {
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            Permissions channelPermissions = e.Channel.PermissionsFor(member);
            CommandContext commandContext = new CommandContext
            {
                BotCoreModule = _botCoreModuleInstance,
                Message = e.Message,
                DiscordMember = member
            };

            CommandListingValue commandListing = _commands[command];
            switch (commandListing.PermissionLevel)
            {
                case BotPermissionLevel.HostOwner:
                    if (e.Author.Id != _botCoreModuleInstance.HostOwnerID)
                    {
                        await e.Channel.SendMessageAsync($"{e.Author.Mention} You are not authorised to use this command!");
                        return;
                    }
                    break;
                case BotPermissionLevel.Admin:
                    if (!channelPermissions.HasFlag(Permissions.Administrator))
                    {
                        await e.Channel.SendMessageAsync($"{e.Author.Mention} This command can only be used by an administrator!");
                        return;
                    }
                    break;
            }

            if (commandListing.Permissions != Permissions.None && !(
                e.Guild.Owner.Id == e.Author.Id ||
                channelPermissions.HasFlag(Permissions.Administrator) ||
                channelPermissions.HasFlag(commandListing.Permissions)))
            {
                await e.Channel.SendMessageAsync($"{e.Author.Mention} You do not have the required permissions to use this command!");
                return;
            }

            _logger.LogDebug($"Running command '{command}' for {e.Author.Username}({e.Author.Id}) in channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}");

            try
            {
                await (commandListing.CommandMethod.Invoke(commandListing.TypeInstance, new object[] { commandContext }) as Task);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running command {command}", ex);
            }
        }
    }
}
