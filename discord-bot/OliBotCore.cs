using System;
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
                Console.WriteLine("No tokens exist!");
                return;
            }
            else if (!TokenHelper.TokenExists(OliBotTokenKey))
            {
                Console.WriteLine($"There isn't a token for OliBot!{Environment.NewLine}Please create a token with the key: '{OliBotTokenKey}'");
                return;
            }

            await Login(TokenHelper.GetTokenValue(OliBotTokenKey));

            OliBotClient.Ready += async (sender) => await SetRandomStatus();

            _statusTimer.Elapsed += async (sender, args) => await SetRandomStatus();
            _statusTimer.Start();

            OliBotClient.MessageCreated += async e =>
            {
                if (e.Author.IsBot)
                    return;
#if DEBUG
                if (e.Channel.Id != 473108499887292447)
#else
                if (e.Channel.Id == 473108499887292447)
#endif
                    return;

                Console.WriteLine($"New message! {e.Author.Username} said: {e.Message.Content}");

                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            await Task.Delay(-1);
        }

        private async Task Login(string token)
        {
            Console.WriteLine("Attempting to login");
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

                Console.WriteLine("Bot ready!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login failed!");
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

            Console.WriteLine($"Set status to {status}");

            await OliBotClient.UpdateStatusAsync(new DiscordGame(status));
        }
    }
}
