﻿using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using DSharpPlus;
using DSharpPlus.Entities;

namespace OliBot
{

    public class BotCore
    {
        static DiscordClient _discord;
        static IConfigurationSection _config;
        static CommandManager _commandManager;

        public static DateTime StartTime { get; private set; }

        public BotCore(IConfigurationRoot configuration)
        {
            _config = configuration.GetSection("BotCore");
            _commandManager = new CommandManager();
        }

        public async Task Start()
        {
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config["Token"],
                TokenType = TokenType.Bot
            });

            _commandManager.Discord = _discord;

            _discord.MessageCreated += async e =>
            {
                if (e.Author.IsBot) return;

                if (e.Message.Content.StartsWith(_config["CommandPrefix"]))
                    await _commandManager.Handle(e.Message);
            };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            _discord.Ready += async e =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                Console.WriteLine("Ready!");
                StartTime = DateTime.Now;
            };

            string status = _config["InitialStatus"];

            await _discord.ConnectAsync(status != null ? new DiscordActivity(status) : null);

            await Task.Delay(-1);
        }
    }
}
