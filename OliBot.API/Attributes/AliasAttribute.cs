using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AliasAttribute : Attribute
    {
        public readonly IReadOnlyList<string> Aliases;

        public AliasAttribute(params string[] aliases)
        {
            Aliases = aliases.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.ToLowerInvariant()).ToList();
        }
    }
}
