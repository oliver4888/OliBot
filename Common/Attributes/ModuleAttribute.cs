using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : DependencyInjectedAttribute
    {
        public ModuleAttribute(Type implements = null) : base(DIType.Singleton, implements) { }
    }
}
