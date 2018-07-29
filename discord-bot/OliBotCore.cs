using DSharpPlus;
using System;
using System.Threading.Tasks;
using discord_bot.Classes;
using System.Timers;

namespace discord_bot
{
    class OliBotCore
    {
        public DiscordClient OliBotClient;

        private static OliBotCore _instance;
        public static OliBotCore Instance => _instance ?? (_instance = new OliBotCore());

        private static string OliBotTokenKey = "olibot";

        private readonly Timer _statusTimer = new Timer(1500);

        static void Main(string[] args)
        {
            Instance.RunBot().ConfigureAwait(false).GetAwaiter().GetResult();
            Console.ReadKey();
        }

        public async Task RunBot()
        {
            await TokenHelper.LoadTokens();

            await Login(TokenHelper.GetTokenValue(OliBotTokenKey));

            if (!TokenHelper.AtLeastOneTokenExists())
            {
                Console.WriteLine("No tokens exist!");
                return;
            }
            else if (!TokenHelper.TokenExists(OliBotTokenKey))
            {
                Console.WriteLine($"There isn't a token for OliBot!{Environment.NewLine}Please create a token with the key: {OliBotTokenKey}");
                return;
            }
            
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

        private async Task<bool> Login(string token)
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
                return false;
            }
            return true;
        }
    }
}
