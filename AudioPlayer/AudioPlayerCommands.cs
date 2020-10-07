using System.Linq;
using System.Threading.Tasks;

using AudioPlayer.Extensions;

using Common;
using Common.Attributes;

using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace AudioPlayer
{
    public class AudioPlayerCommands
    {
        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Join(CommandContext ctx) => await TryJoinVoiceChannel(ctx);

        [Command(disableDMs: true, groupName: "Audio Player")]
        public void Leave(CommandContext ctx)
        {
            VoiceNextConnection vnc = ctx.GetVoiceNext().GetConnection(ctx.Guild);
            if (vnc != null)
                vnc.Disconnect();
        }

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Play(CommandContext ctx, [RemainingText] string track)
        {
            if (await TryJoinVoiceChannel(ctx))
            {
                // Play audio
            }
        }

        private async Task<bool> TryJoinVoiceChannel(CommandContext ctx)
        {
            VoiceNextExtension vne = ctx.GetVoiceNext();
            VoiceNextConnection vnc = vne.GetConnection(ctx.Guild);

            DiscordChannel chn = ctx.Member?.VoiceState?.Channel;

            if (vnc != null && vnc.Channel.Users.Count() != 1)
            {
                if (chn != null && vnc.Channel.Id == chn.Id)
                    await ctx.Message.RespondAsync("I am already in that channel!");
                else
                    await ctx.Message.RespondAsync("I am already in a channel for this guild!");
                return false;
            }
            else if (vnc != null)
                vnc.Disconnect();

            if (chn == null)
            {
                await ctx.Message.RespondAsync("You need to be in a voice channel!");
                return false;
            }

            await chn.ConnectAsync();
            return true;
        }
    }
}
