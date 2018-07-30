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
        public static DiscordClient OliBotClient;

        private static OliBotCore _instance;
        public static OliBotCore Instance => _instance ?? (_instance = new OliBotCore());

        private static readonly string OliBotTokenKey = "olibot";

        // Eventually I will move everything into a OliBotConfig.xml or something like that.
        private static readonly string _tokenFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tokens.xml");
        private static readonly string _statusesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statuses.txt");

        private readonly Timer _statusTimer = new Timer(30 * (60 * 1000)); // Every 30 minutes
        private static List<string> _statuses;
        private static Queue<string> _statusHistoryQueue = new Queue<string>();

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

#if DEBUG
            await SetStatus("Getting an upgrade");
#else
            OliBotClient.Ready += async (sender) => await SetRandomStatus();

            _statusTimer.Elapsed += async (sender, args) => await SetRandomStatus();
            _statusTimer.Start();
#endif

            OliBotClient.MessageCreated += async e =>
            {
#if DEBUG
                if (e.Channel.Id != 473108499887292447)
#else
                if (e.Channel.Id == 473108499887292447)
#endif
                    return;

                string userName = e.Guild.GetMemberAsync(e.Author.Id).Result.Nickname;

                if (userName == null)
                {
                    userName = $"{e.Author.Username}#{e.Author.Discriminator}";
                }
                else
                {
                    userName += $"({e.Author.Username}#{e.Author.Discriminator})";
                }

                WriteToConsole("NewMessage", $"{e.Guild.Name}/{e.Channel.Name}: {userName} said: {e.Message.Content}");

                if (e.Author.IsBot)
                    return;

                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync($"{e.Author.Mention}, pong!");

                if (e.Message.Content.ToLower() == "!src")
                    await e.Message.RespondAsync($"{e.Author.Mention}, The OliBot source code can be found at:\r\nhttps://gitlab.com/Oliver4888/discord-bot");

                if (e.Message.Content.ToLower().Contains("olly") || e.Message.Content.ToLower().Contains("ollie"))
                    await e.Message.RespondAsync($"{e.Author.Mention} the correct spelling is \"Oli\"");
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

            string status;

            do
            {
                status = _statuses[_random.Next(_statuses.Count)];
            } while (status.Trim() == "" && _statusHistoryQueue.Contains(status));

            _statusHistoryQueue.Enqueue(status);

            if (_statusHistoryQueue.Count > _statuses.Count / 3)
                _statusHistoryQueue.Dequeue();

            WriteToConsole("Status", $"Set status to {status}");

            await SetStatus(status);
        }

        private async Task SetStatus(string status)
        {
            await OliBotClient.UpdateStatusAsync(new DiscordGame(status));
        }

        public void WriteToConsole(string category, string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}{(DateTime.Now.IsDaylightSavingTime() ? " +01:00" : "")}] [OliBot] [{category}] {message}");
        }
    }
}
