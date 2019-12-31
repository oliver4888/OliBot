﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using Common;

namespace OliBot
{
    public class CommandManager
    {
        IDictionary<string, MethodInfo> _commands = new Dictionary<string, MethodInfo>();
        public DiscordClient Discord;

        public CommandManager()
        {
            IEnumerable<MethodInfo> methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0);

            foreach (MethodInfo method in methods)
                _commands.Add(method.Name.ToLower(), method);

            Console.WriteLine($"Loaded {_commands.Count()} commands!");
            Console.WriteLine($"Loaded commands: {string.Join(", ", _commands.Keys)}");
        }

        public async Task Handle(DiscordMessage message)
        {
            string[] sections = message.Content.Substring(1).Split("");
            await (Task)_commands[sections[0].ToLower()]?.Invoke(null, new object[] { Discord, message });
        }
    }
}
