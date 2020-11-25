using System;
using DSharpPlus;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequiredPermissionsAttribute : Attribute
    {
        public readonly Permissions Permissions;

        public RequiredPermissionsAttribute(Permissions permissions) => Permissions = permissions;
    }
}
