using DSharpPlus;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IBotCoreModule
    {
        public static DiscordClient DiscordClient { get; private set; }
        public static ICommandHandler CommandHandler { get; private set; }

        public Task Start();
    }
}
