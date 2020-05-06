using System;
using System.Reflection;

namespace Common.Interfaces
{
    public interface ICommandParameter
    {
        public ParameterInfo ParameterInfo { get; }
        public Type Type { get; }
        public string Descriptions { get; }
        public bool Required { get; }
    }
}
