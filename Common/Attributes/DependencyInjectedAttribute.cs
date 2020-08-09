using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DependencyInjectedAttribute : Attribute
    {
        public readonly DIType Type;

        public DependencyInjectedAttribute(DIType dIType) => Type = dIType;
    }
}
