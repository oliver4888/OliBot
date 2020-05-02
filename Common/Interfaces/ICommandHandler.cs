using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ICommandHandler
    {
        public IReadOnlyCollection<ICommand> Commands { get; }

        public void RegisterCommands<T>();
        public void RegisterCommands(Type commandClass);
    }
}
