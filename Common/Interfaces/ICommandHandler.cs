using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ICommandHandler
    {
        public string CommandPrefix { get; }
        public IReadOnlyCollection<ICommand> Commands { get; }

        public void RegisterCommands<T>();
        public void RegisterCommands(Type commandClass);
    }
}
