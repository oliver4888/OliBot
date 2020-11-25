using System;
using System.Reflection;

namespace OliBot.API.Interfaces
{
    public interface ICommandParameter
    {
        public ParameterInfo ParameterInfo { get; }
        public Type Type { get; }
        public string Description { get; }
        public bool Required { get; }
        public bool RemainingText { get; }
        public bool FromServices { get; }
    }
}
