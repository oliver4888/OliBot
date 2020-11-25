using System;
using DSharpPlus;
using System.Threading.Tasks;

namespace OliBot.API.Interfaces
{
    public interface IBotCoreModule
    {
        public DiscordClient DiscordClient { get; }
        public ICommandHandler CommandHandler { get; }
        public DateTime StartTime { get; }
        public ulong HostOwnerID { get; }

        public Task Start();
    }
}
