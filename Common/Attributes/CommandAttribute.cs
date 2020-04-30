using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string CommandName;
        public CommandAttribute(string commandName = "")
        {
            CommandName = commandName.ToLowerInvariant();
        }
    }
}
