using System;
using DSharpPlus;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IBotCoreModule
    {
        public DiscordClient DiscordClient { get; }
        public ICommandHandler CommandHandler { get; }
        public DateTime StartTime { get; }

        public Task Start();
    }
}
