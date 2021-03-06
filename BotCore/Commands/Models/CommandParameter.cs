﻿using System;
using System.Reflection;

using OliBot.API.Attributes;
using OliBot.API.Interfaces;

namespace BotCore.Commands.Models
{
    public class CommandParameter : ICommandParameter
    {
        public ParameterInfo ParameterInfo { get; private set; }

        public Type Type => ParameterInfo.ParameterType;

        public string Description { get; private set; }

        public bool Required => !ParameterInfo.IsOptional;

        public bool RemainingText { get; private set; }
        public bool FromServices { get; private set; }

        public CommandParameter(ParameterInfo parameter)
        {
            ParameterInfo = parameter;

            Description = ParameterInfo.IsDefined(typeof(DescriptionAttribute), false) ?
                ParameterInfo.GetCustomAttribute<DescriptionAttribute>().DescriptionText : DescriptionAttribute.NoDescriptionText;

            RemainingText = ParameterInfo.IsDefined(typeof(RemainingTextAttribute), false);

            FromServices = ParameterInfo.IsDefined(typeof(FromServicesAttribute), false);
        }
    }
}
