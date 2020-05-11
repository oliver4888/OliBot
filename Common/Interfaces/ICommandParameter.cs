using System;
using System.Reflection;

namespace Common.Interfaces
{
    public interface ICommandParameter
    {
        public ParameterInfo ParameterInfo { get; }
        public Type Type { get; }
        public string Description { get; }
        public bool Required { get; }
    }
}
