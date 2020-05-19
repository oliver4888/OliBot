using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string CommandName;
        public readonly bool Hidden;
        public readonly BotPermissionLevel PermissionLevel;
        public readonly bool DisableDMs;

        public CommandAttribute(string commandName = "", bool hidden = false, BotPermissionLevel permissionLevel = BotPermissionLevel.Everyone, bool disableDMs = false)
        {
            CommandName = commandName.ToLowerInvariant();
            Hidden = hidden;
            PermissionLevel = permissionLevel;
            DisableDMs = disableDMs;
        }
    }
}
