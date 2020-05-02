using Common;
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

        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Hidden { get; private set; }
        public BotPermissionLevel PermissionLevel { get; private set; }
        public Permissions Permissions { get; private set; }

        public Command(Type type, ref object typeInstance, MethodInfo commandMethod)
        {
            Type = type;
            CommandMethod = commandMethod;

            CommandAttribute commandAttribute = commandMethod.GetCustomAttribute<CommandAttribute>();

            Name = commandAttribute.CommandName == "" ? commandMethod.Name : commandAttribute.CommandName;
            Hidden = commandAttribute.Hidden;
            PermissionLevel = commandAttribute.PermissionLevel;

            Description = commandMethod.IsDefined(typeof(DescriptionAttribute), false) ?
                commandMethod.GetCustomAttribute<DescriptionAttribute>().DescriptionText : "No description provided.";

            Permissions = commandMethod.IsDefined(typeof(RequiredPermissionsAttribute), false) ?
                commandMethod.GetCustomAttribute<RequiredPermissionsAttribute>().Permissions : Permissions.None;

            IList<Type> args = new List<Type>();
            foreach (var param in commandMethod.GetParameters())
                args.Add(param.ParameterType);
            args.Add(commandMethod.ReturnType);
            Type delDecltype = Expression.GetDelegateType(args.ToArray());

            MethodDelegate = Delegate.CreateDelegate(delDecltype, typeInstance, commandMethod.Name);
        }
    }
}
