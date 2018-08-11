using System;
using System.Threading.Tasks;
using discord_bot.Classes;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using NLog;

namespace discord_bot
{
    class OliBotCore
    {
        public static DiscordClient OliBotClient;

        private static OliBotCore _instance;
        public static OliBotCore Instance => _instance ?? (_instance = new OliBotCore());

        private static CommandsNextModule _commands;

        private static readonly string OliBotTokenKey = "olibot";

        // Eventually I will move everything into a OliBotConfig.xml or something like that.
        private static readonly string _tokenFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tokens.xml");
        private static readonly string _statusesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statuses.txt");

        private readonly Timer _statusTimer = new Timer(30 * (60 * 1000)); // Every 30 minutes
        private static List<string> _statuses;
        private static Queue<string> _statusHistoryQueue = new Queue<string>();

        private static Random _random = new Random();

        public static ulong Oliver4888Id = 149509587425296384;

        public static ulong DevGuildId = 223546785598144512;
        public static ulong DevChannelId = 473108499887292447;

        public static ulong CSGOStratGuildId = 306171979583717386;

        public static Logger Log = LogManager.GetLogger("OliBot");

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
                Log.Warn("Tokens| No tokens exist!");
                return;
            }
            else if (!TokenHelper.TokenExists(OliBotTokenKey))
            {
                Log.Warn($"Tokens| There isn't a token for OliBot! Please create a token with the key: '{OliBotTokenKey}'");
                return;
            }

            await Login(TokenHelper.GetTokenValue(OliBotTokenKey));

            await Task.Delay(-1);
        }

        private async Task Login(string token)
        {
            Log.Info("General| Attempting to login");
            try
            {
                OliBotClient = new DiscordClient(new DiscordConfiguration
                {
#if DEBUG
                    UseInternalLogHandler = true,
                    LogLevel = DSharpPlus.LogLevel.Debug,
#endif
                    Token = token,
                    TokenType = TokenType.Bot
                });

                _commands = OliBotClient.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = "?",
                    CaseSensitive = false
                });

                _commands.RegisterCommands<OliBotCommands>();

                OliBotClient.MessageCreated += async e => await OliBot_MessageCreated(e);
                OliBotClient.Ready += async e => await OliBot_Ready(e);

                await OliBotClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Login failed!");
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

            Log.Info($"Status| Set status to {status}");

            await SetStatus(status);
        }

        private async Task SetStatus(string status)
        {
            await OliBotClient.UpdateStatusAsync(new DiscordGame(status));
        }

        private async Task OliBot_Ready(ReadyEventArgs e)
        {
            Log.Info("General| Bot ready!");
#if DEBUG
            await SetStatus("Getting an upgrade");
#else
            await SetRandomStatus();

            _statusTimer.Elapsed += async (sender, args) => await SetRandomStatus();
            _statusTimer.Start();
#endif
        }

        private async Task OliBot_MessageCreated(MessageCreateEventArgs e)
        {
#if DEBUG
            if (e.Channel.Id != DevChannelId)
#else
            if (e.Channel.Id == DevChannelId)
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

            Log.Info($"NewMessage| {e.Guild.Name}/{e.Channel.Name}: {userName} said: {e.Message.Content}");

            if (e.Author.IsBot)
                return;

            if (e.Message.Content.ToLower().Contains("olly") || e.Message.Content.ToLower().Contains("ollie"))
                await e.Message.RespondAsync($"{e.Author.Mention} the correct spelling is \"Oli\"");
        }
    }
}
