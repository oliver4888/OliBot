using System;
using System.Linq;
using System.Reflection;
using Common.Attributes;
using Common.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace BotCoreModule
{
    public class CommandHandler : ICommandHandler
    {
        readonly ILogger<CommandHandler> _logger;
        readonly IBotCoreModule _botCoreModuleInstance;

        public CommandHandler(ILogger<CommandHandler> logger, IBotCoreModule botCoreModuleInstance)
        {
            _logger = logger;
            _botCoreModuleInstance = botCoreModuleInstance;
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

            // Register commands
        }
    }
}
