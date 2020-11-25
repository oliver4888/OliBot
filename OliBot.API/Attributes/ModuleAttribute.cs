using System;

namespace OliBot.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : DependencyInjectedAttribute
    {
        public ModuleAttribute(Type implements = null) : base(DIType.Singleton, implements) { }
    }
}
