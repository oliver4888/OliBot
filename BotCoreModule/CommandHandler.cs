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

namespace BotCoreModule
{
    public class CommandHandler : ICommandHandler
    {
        // This will be moved to a db and configurable per server 
        const string _commandPrefix = "??";

        readonly ILogger<CommandHandler> _logger;
        readonly IBotCoreModule _botCoreModuleInstance;

        readonly IDictionary<string, CommandListingValue> _commands = new Dictionary<string, CommandListingValue>();

        public IEnumerable<string> CommandNames { get { return _commands.Keys; } }

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
                _commands.Add(ca.CommandName == "" ? command.Name.ToLowerInvariant() : ca.CommandName, new CommandListingValue(commandClass, commandClassInstance, command));
            }

            _logger.LogInformation($"Registered {_commands.Count()} command(s) for Type {commandClass.FullName}");
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !e.Message.Content.StartsWith(_commandPrefix)) return;

            string[] messageParts = e.Message.Content.Remove(0, _commandPrefix.Length).Split(" ");

            string command = messageParts[0].ToLowerInvariant();

            if (_commands.ContainsKey(command))
            {
                CommandListingValue commandListing = _commands[command];
                switch(commandListing.PermissionLevel)
                {
                    case BotPermissionLevel.HostOwner:
                        if (e.Author.Id != _botCoreModuleInstance.HostOwnerID)
                        {
                            await e.Channel.SendMessageAsync($"{e.Author.Mention} You are not authorised to use this command!");
                            return;
                        }
                        break;
                    case BotPermissionLevel.Admin:
                        if (!e.Channel.PermissionsFor(await e.Guild.GetMemberAsync(e.Author.Id)).HasFlag(Permissions.Administrator))
                        {
                            await e.Channel.SendMessageAsync($"{e.Author.Mention} This command can only be used by an administrator!");
                            return;
                        }
                        break;
                }
                try
                {
                    await (commandListing.CommandMethod.Invoke(commandListing.TypeInstance, new object[] { new CommandContext { BotCoreModule = _botCoreModuleInstance, Message = e.Message } }) as Task);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error running command {command}", ex);
                }
            }
        }
    }
}
