using System;

namespace OliBot.API.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class RemainingTextAttribute : Attribute
    {
    }
}