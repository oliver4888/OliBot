using System;
using System.Collections.Generic;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class UserWhitelistAttribute : Attribute
    {
        public readonly IReadOnlyCollection<ulong> UserIds;

        public UserWhitelistAttribute(params ulong[] userIds)
        {
            UserIds = userIds;
        }
    }
}
