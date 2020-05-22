using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        public readonly Type Implements;

        public ModuleAttribute(Type implements = null)
        {
            Implements = implements;
        }
    }
}
