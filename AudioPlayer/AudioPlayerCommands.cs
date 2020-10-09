using System.Threading.Tasks;

using AudioPlayer.Extensions;

using Common;
using Common.Attributes;

using DSharpPlus.VoiceNext;

namespace AudioPlayer
{
    public class AudioPlayerCommands
    {
        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Join(CommandContext ctx, [FromServices] AudioPlayerModule audioPlayerModule) => await audioPlayerModule.TryJoinVoiceChannel(ctx);

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Leave(CommandContext ctx)
        {
            VoiceNextConnection vnc = ctx.GetVoiceNext().GetConnection(ctx.Guild);
            if (ctx.Member.VoiceState.Channel == null)
                await ctx.Message.RespondAsync("You are not in a voice channel!");
            else if (vnc.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                await ctx.Message.RespondAsync("I am not in your channel!");
            else if (vnc != null)
                vnc.Disconnect();
        }

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Play(CommandContext ctx, [RemainingText] string track, [FromServices] AudioPlayerModule audioPlayerModule)
        {
            bool isConnected = ctx.GetVoiceNext().GetConnection(ctx.Guild) != null;

            if (isConnected || await audioPlayerModule.TryJoinVoiceChannel(ctx))
            {
                // Play audio
            }
        }
    }
}
