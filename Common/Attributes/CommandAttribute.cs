using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string CommandName;
        public readonly bool Hidden;
        public readonly BotPermissionLevel PermissionLevel;

        public CommandAttribute(string commandName = "", bool hidden = false, BotPermissionLevel permissionLevel = BotPermissionLevel.Everyone)
        {
            CommandName = commandName.ToLowerInvariant();
            Hidden = hidden;
            PermissionLevel = permissionLevel;
        }
    }
}
