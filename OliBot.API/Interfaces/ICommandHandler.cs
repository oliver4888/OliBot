﻿using System;
using System.Collections.Generic;

namespace OliBot.API.Interfaces
{
    public interface ICommandHandler
    {
        public string CommandPrefix { get; }
        public IReadOnlyCollection<ICommand> Commands { get; }

        public void RegisterCommands<T>();
        public void RegisterCommands(Type commandClass);
        
        public void RegisterConverter<T>();
        public void RegisterConverter(Type converter);

        public bool TryGetCommand(string commandName, out ICommand command);
    }
}
