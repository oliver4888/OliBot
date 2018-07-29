﻿using System;
using System.Threading.Tasks;
using discord_bot.Classes;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;

namespace discord_bot
{
    class OliBotCore
    {
        public DiscordClient OliBotClient;

        private static OliBotCore _instance;
        public static OliBotCore Instance => _instance ?? (_instance = new OliBotCore());

        private static string OliBotTokenKey = "olibot";

        // Eventually I will move everything into a OliBotConfig.xml or something like that.
        private static string _tokenFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tokens.xml");
        private static string _statusesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statuses.txt");

        private readonly Timer _statusTimer = new Timer(30 * (60 * 1000)); // Every 30 minutes
        private static List<string> _statuses;

        private static Random _random = new Random();

        static void Main(string[] args)
        {
            Instance.RunBot().ConfigureAwait(false).GetAwaiter().GetResult();
            Console.ReadKey();
        }

        public async Task RunBot()
        {
            await TokenHelper.LoadTokens(_tokenFile);
            await LoadStatuses();

            if (!TokenHelper.AtLeastOneTokenExists())
            {
                WriteToConsole("Tokens", "No tokens exist!");
                return;
            }
            else if (!TokenHelper.TokenExists(OliBotTokenKey))
            {
                WriteToConsole("Tokens", $"There isn't a token for OliBot!{Environment.NewLine}Please create a token with the key: '{OliBotTokenKey}'");
                return;
            }

            await Login(TokenHelper.GetTokenValue(OliBotTokenKey));

            OliBotClient.Ready += async (sender) => await SetRandomStatus();

            _statusTimer.Elapsed += async (sender, args) => await SetRandomStatus();
            _statusTimer.Start();

            OliBotClient.MessageCreated += async e =>
            {
#if DEBUG
                if (e.Channel.Id != 473108499887292447)
#else
                if (e.Channel.Id == 473108499887292447)
#endif
                    return;

                WriteToConsole("NewMessage", $"{e.Guild.Name}/{e.Channel.Name}! {e.Author.Username} said: {e.Message.Content}");
                
                if (e.Author.IsBot)
                    return;

                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");

                if (e.Message.Content.ToLower() == "!src")
                    await e.Message.RespondAsync("The OliBot source code can be found at:\r\nhttps://gitlab.com/Oliver4888/discord-bot");
            };

            await Task.Delay(-1);
        }

        private async Task Login(string token)
        {
            WriteToConsole("General", "Attempting to login");
            try
            {
                OliBotClient = new DiscordClient(new DiscordConfiguration
                {
#if DEBUG
                    UseInternalLogHandler = true,
                    LogLevel = LogLevel.Debug,
#endif
                    Token = token,
                    TokenType = TokenType.Bot
                });

                await OliBotClient.ConnectAsync();

                WriteToConsole("General", "Bot ready!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("General", "Login failed!");
                Console.WriteLine(ex);
            }
            return;
        }

        private async Task LoadStatuses()
        {
            if (!File.Exists(_statusesFile))
            {
                string[] defaultStatuses = new string[]
                {
                    "Visual Studio",
                    "Visual Studio Code",
                    "CS:GO",
                    "Counter Strike: Global Offensive",
                    "Google Chrome",
                    "Coogle Ghrome",
                    "Nothing",
                    "Club Pengiun",
                    "Roblox",
                    "Runescape 2007",
                    "Minecraft"
                };
                File.WriteAllLines(_statusesFile, defaultStatuses);
                _statuses = new List<string>(defaultStatuses);
                return;
            }
            
            using (StreamReader reader = File.OpenText(_statusesFile))
            {
                string fileText = await reader.ReadToEndAsync();
                _statuses = new List<string>(fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
            }
        }

        private async Task SetRandomStatus()
        {
            if (_statuses == null)
                return;

            string status = _statuses[_random.Next(_statuses.Count)];

            WriteToConsole("Status", $"Set status to {status}");

            await OliBotClient.UpdateStatusAsync(new DiscordGame(status));
        }

        public void WriteToConsole(string category, string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {(DateTime.Now.IsDaylightSavingTime() ? "+01:00" : "")}] [OliBot] [{category}] {message}");
        }
    }
}