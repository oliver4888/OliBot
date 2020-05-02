using System;
using DSharpPlus;
using System.Reflection;

namespace Common.Interfaces
{
    public interface ICommand
    {
        public Type Type { get; }
        public MethodInfo CommandMethod { get; }
        public Delegate MethodDelegate { get; }

        public string Name { get; }
        public string Description { get; }
        public bool Hidden { get; }
        public BotPermissionLevel PermissionLevel { get; }
        public Permissions Permissions { get; }
    }
}
