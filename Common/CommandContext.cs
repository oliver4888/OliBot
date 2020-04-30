using Common.Interfaces;
using DSharpPlus.Entities;

namespace Common
{
    public class CommandContext
    {
        public IBotCoreModule BotCoreModule;
        public DiscordMessage Message;
    }
}
