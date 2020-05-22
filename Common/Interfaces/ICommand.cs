﻿using System;
using DSharpPlus;
using System.Reflection;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ICommand
    {
        public Type Type { get; }
        public MethodInfo CommandMethod { get; }
        public Delegate MethodDelegate { get; }
        public IReadOnlyList<ICommandParameter> Parameters { get; }

        public string Name { get; }
        public IReadOnlyList<string> Triggers { get; }
        public string Description { get; }
        public bool Hidden { get; }
        public bool DisableDMs { get; }

        public BotPermissionLevel PermissionLevel { get; }
        public Permissions Permissions { get; }
    }
}
