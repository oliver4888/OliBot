﻿using System;

namespace OliBot.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string CommandName;
        public readonly bool Hidden;
        public readonly BotPermissionLevel PermissionLevel;
        public readonly bool DisableDMs;
        public readonly string GroupName;

        public CommandAttribute(string commandName = "", bool hidden = false, BotPermissionLevel permissionLevel = BotPermissionLevel.Everyone, bool disableDMs = false, string groupName = "General")
        {
            CommandName = commandName.ToLowerInvariant();
            Hidden = hidden;
            PermissionLevel = permissionLevel;
            DisableDMs = disableDMs;
            GroupName = groupName;
        }
    }
}
