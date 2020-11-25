using OliBot.API;
using OliBot.API.Attributes;

namespace BotCore
{
    [DependencyInjected(DIType.Options)]
    public class BotCore
    {
        public string Token { get; set; }
        public string InitialStatus { get; set; }
        public ulong HostOwnerID { get; set; }
        public string CommandPrefix { get; set; }
    }
}
