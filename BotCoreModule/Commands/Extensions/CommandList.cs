using System;
using System.Linq;
using Common.Interfaces;
using System.Collections.Generic;

namespace BotCoreModule.Commands.Extensions
{
    public static class CommandList
    {
        public static bool TryGetCommand(this IList<ICommand> commands, string commandName, out ICommand command) =>
            (command = commands.FirstOrDefault(c => c.Triggers.Contains(commandName))) == null ? false : true;
    }
}
