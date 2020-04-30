using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DescriptionAttribute : Attribute
    {
        public readonly string DescriptionText;
        public DescriptionAttribute(string descriptionText = "")
        {
            DescriptionText = descriptionText;
        }
    }
}
