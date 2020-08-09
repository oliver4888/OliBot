using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : DependencyInjectedAttribute
    {
        public readonly Type Implements;

        public ModuleAttribute(Type implements = null) : base(DIType.Singleton) => Implements = implements;
    }
}
