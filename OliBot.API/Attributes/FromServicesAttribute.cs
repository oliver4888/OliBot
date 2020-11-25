using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromServicesAttribute : Attribute
    {
    }
}
