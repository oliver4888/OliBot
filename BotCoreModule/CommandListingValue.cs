using Common;
using System;
using Common.Attributes;
using System.Reflection;

namespace BotCoreModule
{
    public class CommandListingValue
    {
        public readonly Type Type;
        public readonly object TypeInstance;
        public readonly MethodInfo CommandMethod;

        public readonly bool Hidden;
        public readonly BotPermissionLevel PermissionLevel;

        public CommandListingValue(Type type, object typeInstance, MethodInfo commandMethod)
        {
            Type = type;
            TypeInstance = typeInstance;
            CommandMethod = commandMethod;

            CommandAttribute commandAttribute = commandMethod.GetCustomAttribute<CommandAttribute>();

            Hidden = commandAttribute.Hidden;
            PermissionLevel = commandAttribute.PermissionLevel;
        }
    }
}
