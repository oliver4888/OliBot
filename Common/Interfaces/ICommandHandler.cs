using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ICommandHandler
    {
        public IEnumerable<string> CommandNames { get; }

        public void RegisterCommands<T>();
        public void RegisterCommands(Type commandClass);
    }
}
