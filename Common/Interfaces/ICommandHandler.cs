using System;

namespace Common.Interfaces
{
    public interface ICommandHandler
    {
        public void RegisterCommands<T>();
        public void RegisterCommands(Type commandClass);
    }
}
