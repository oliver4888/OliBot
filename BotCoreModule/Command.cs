﻿using Common;
using System;
using DSharpPlus;
using System.Linq;
using Common.Attributes;
using Common.Interfaces;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace BotCoreModule
{
    public class Command : ICommand
    {
        public Type Type { get; private set; }
        public MethodInfo CommandMethod { get; private set; }
        public Delegate MethodDelegate { get; private set; }
        readonly IList<ICommandParameter> _parameters = new List<ICommandParameter>();
        public IReadOnlyList<ICommandParameter> Parameters => _parameters as IReadOnlyList<ICommandParameter>;

        public string Name { get; private set; }
        readonly IList<string> _triggers = new List<string>();
        public IReadOnlyList<string> Triggers => _triggers as IReadOnlyList<string>;
        public string Description { get; private set; }
        public bool Hidden { get; private set; }
        public BotPermissionLevel PermissionLevel { get; private set; }
        public Permissions Permissions { get; private set; }

        public Command(Type type, ref object typeInstance, MethodInfo commandMethod)
        {
            Type = type;
            CommandMethod = commandMethod;

            CommandAttribute commandAttribute = commandMethod.GetCustomAttribute<CommandAttribute>();

            Name = (commandAttribute.CommandName == "" ? commandMethod.Name : commandAttribute.CommandName).ToLowerInvariant();
            _triggers.Add(Name);
            Hidden = commandAttribute.Hidden;
            PermissionLevel = commandAttribute.PermissionLevel;

            if (commandMethod.IsDefined(typeof(AliasAttribute), false))
                _triggers = _triggers.Concat(commandMethod.GetCustomAttribute<AliasAttribute>().Aliases).ToList();

            Description = commandMethod.IsDefined(typeof(DescriptionAttribute), false) ?
                commandMethod.GetCustomAttribute<DescriptionAttribute>().DescriptionText : "No description provided.";

            Permissions = commandMethod.IsDefined(typeof(RequiredPermissionsAttribute), false) ?
                commandMethod.GetCustomAttribute<RequiredPermissionsAttribute>().Permissions : Permissions.None;

            IList<Type> args = new List<Type>();
            foreach (ParameterInfo param in commandMethod.GetParameters())
                args.Add(param.ParameterType);
            args.Add(commandMethod.ReturnType);
            Type delDecltype = Expression.GetDelegateType(args.ToArray());

            MethodDelegate = Delegate.CreateDelegate(delDecltype, typeInstance, commandMethod.Name);
        }
    }
}
