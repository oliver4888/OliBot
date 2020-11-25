using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class DescriptionAttribute : Attribute
    {
        public readonly string DescriptionText;

        public static readonly string NoDescriptionText = "No description provided.";

        public DescriptionAttribute(string descriptionText = "")
        {
            DescriptionText = descriptionText;
        }
    }
}
