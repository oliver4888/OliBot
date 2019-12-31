using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

namespace OliBot.Commands
{
    public interface ICommandManager
    {
        DiscordClient Discord { get; set; }

        Task Handle(DiscordMessage message);
    }
}
