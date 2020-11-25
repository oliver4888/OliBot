using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DependencyInjectedAttribute : Attribute
    {
        public readonly DIType Type;
        public readonly Type Implements;

        public DependencyInjectedAttribute(DIType dIType, Type implements = null) => (Type, Implements) = (dIType, implements);
    }
}
