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
using System.Text.RegularExpressions;

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

        public readonly Timer StatusTimer = new Timer(30 * (60 * 1000)); // Every 30 minutes
        private static List<string> _statuses;
        private static Queue<string> _statusHistoryQueue = new Queue<string>();

        private static Random _random = new Random();

        public static ulong Oliver4888Id = 149509587425296384;

        public static ulong DevGuildId = 223546785598144512;
        public static ulong DevChannelId = 473108499887292447;

        public static ulong CSGOStratGuildId = 306171979583717386;

        public static Logger Log = LogManager.GetLogger("OliBot");

        private static OliBotEvents _events = new OliBotEvents();

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

                OliBotClient.MessageCreated += async e => await _events.OliBot_MessageCreated(e);
                OliBotClient.GuildCreated += async e => await _events.OliBot_GuildCreated(e);
                OliBotClient.GuildDeleted += async e => await _events.OliBot_GuildDeleted(e);

                OliBotClient.ChannelCreated += async e => await _events.OliBotCore_ChannelCreated(e);

                OliBotClient.GuildMemberAdded += async e => await _events.OliBot_GuildMemberAdded(e);
                OliBotClient.GuildMemberRemoved += async e => await _events.OliBot_GuildMemberRemoved(e);

                OliBotClient.Ready += async e => await _events.OliBot_Ready(e);

                await OliBotClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Login failed!");
            }

            try
            {
                RedditHelper.Login(
                    TokenHelper.GetTokenValue("redditUsername"),
                    TokenHelper.GetTokenValue("redditPassword"),
                    TokenHelper.GetTokenValue("redditClientId"),
                    TokenHelper.GetTokenValue("redditSecret")
                );
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Reddit login failed!");
            }

            return;
        }

        public string GetUserNameFromDiscordUser(DiscordGuild guild, DiscordUser user)
        {
            string userName = guild.GetMemberAsync(user.Id).Result.Nickname;

            if (userName == null)
            {
                userName = $"{user.Username}#{user.Discriminator}";
            }
            else
            {
                userName += $"({user.Username}#{user.Discriminator})";
            }
            return $"{userName}/{user.Id}";
        }

        #region Status Helpers

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
            _statuses = CleanStatuses(_statuses);
        }

        private bool SaveStatuses()
        {
            try
            {
                File.WriteAllLines(_statusesFile, _statuses);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to save to status file");
            }
            return false;
        }

        public List<string> CleanStatuses(List<string> statuses)
        {
            for (int i = statuses.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(statuses[i]))
                {
                    statuses.Remove(statuses[i]);
                }
            }
            return statuses;
        }

        public bool AddStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            _statuses.Add(status);
            return SaveStatuses();
        }

        public async Task SetRandomStatus()
        {
            if (_statuses == null)
                return;

            string status;

            do
            {
                status = _statuses[_random.Next(_statuses.Count)];
            } while (string.IsNullOrWhiteSpace(status) && _statusHistoryQueue.Contains(status));

            _statusHistoryQueue.Enqueue(status);

            if (_statusHistoryQueue.Count > _statuses.Count / 3)
                _statusHistoryQueue.Dequeue();

            await SetStatus(status);
        }

        public async Task SetStatus(string status)
        {
            Log.Info($"Status| Set status to {status}");
            await OliBotClient.UpdateStatusAsync(new DiscordGame(status));
        }

        #endregion

        #region Ensure Oli is in guild methods

        public async Task EnsureOliInGuilds()
        {
            foreach (KeyValuePair<ulong, DiscordGuild> entry in OliBotClient.Guilds)
            {
                await EnsureOliInGuild(entry.Value);
            }
        }

        public async Task<bool> EnsureOliInGuild(DiscordGuild guild)
        {
            if (guild.GetMemberAsync(Oliver4888Id).Result == null)
            {
                await UnauthorisedBotUse(guild);
                return false;
            }
            return true;
        }

        public async Task UnauthorisedBotUse(DiscordGuild guild)
        {
            Log.Info($"Oliver4888 isn't in guild {guild.Name}({guild.Id})");
            await guild.GetDefaultChannel().SendMessageAsync("You are not authorised to use this bot!");
            await guild.LeaveAsync();
        }

        #endregion

        #region Random Helpers

        public async Task<DiscordRole> GetMutedRole(DiscordGuild guild)
        {
            DiscordRole muted = null;

            foreach (DiscordRole role in guild.Roles)
            {
                if (role.Name.ToLower() == "muted")
                {
                    muted = role;
                    break;
                }
            }

            if (muted == null)
                muted = await guild.CreateRoleAsync("Muted", mentionable: true);

            return muted;
        }

        #endregion
    }

    public static class Extensions
    {
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
