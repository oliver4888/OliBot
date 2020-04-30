using Common;
using System;
using System.Linq;
using System.Reflection;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.EventArgs;
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

        readonly IDictionary<string, KeyValuePair<object, MethodInfo>> _commands = new Dictionary<string, KeyValuePair<object, MethodInfo>>();

        public IEnumerable<string> CommandNames { get { return _commands.Keys; } }

        public CommandHandler(ILogger<CommandHandler> logger, IBotCoreModule botCoreModuleInstance)
        {
            _logger = logger;
            _botCoreModuleInstance = botCoreModuleInstance;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            _botCoreModuleInstance.DiscordClient.MessageCreated += async e => OnMessageCreated(e);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
                _commands.Add(command.Name.ToLowerInvariant(), new KeyValuePair<object, MethodInfo>(commandClassInstance, command));

            _logger.LogInformation($"Registered {_commands.Count()} commands for Type {commandClass.FullName}");
        }

        private void OnMessageCreated(MessageCreateEventArgs e)
        {
            if (!e.Message.Content.StartsWith(_commandPrefix)) return;

            string[] messageParts = e.Message.Content.Remove(0, _commandPrefix.Length).Split(" ");

            string command = messageParts[0].ToLowerInvariant();

            if (_commands.ContainsKey(command))
            {
                KeyValuePair<object, MethodInfo> commandPair = _commands[command];
                try
                {
                    commandPair.Value.Invoke(commandPair.Key, new object[] { new CommandContext { BotCoreModule = _botCoreModuleInstance, Message = e.Message } });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error running command {command}", ex);
                }
            }
        }
    }
}
