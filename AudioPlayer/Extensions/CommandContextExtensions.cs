using Common;

using DSharpPlus.VoiceNext;

namespace AudioPlayer.Extensions
{
    public static class CommandContextExtensions
    {
        public static VoiceNextExtension GetVoiceNext(this CommandContext ctx) => ctx.BotCoreModule.DiscordClient.GetVoiceNext();
    }
}
