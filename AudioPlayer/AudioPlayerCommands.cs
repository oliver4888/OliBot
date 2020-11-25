using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using AudioPlayer.Extensions;

using OliBot.API;
using OliBot.API.Attributes;
using OliBot.API.Extensions;
using OliBot.API.Interfaces;

using DSharpPlus.Entities;
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
            if (vnc == null)
                await ctx.Message.RespondAsync("I'm not in a voice channel!");
            else if (ctx.Member.VoiceState.Channel == null)
                await ctx.Message.RespondAsync("You are not in a voice channel!");
            else if (vnc.TargetChannel.Id != ctx.Member.VoiceState.Channel.Id)
                await ctx.Message.RespondAsync("I am not in your channel!");
            else if (vnc != null)
                vnc.Disconnect();
        }

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Play(CommandContext ctx, [RemainingText] string trackName, [FromServices] AudioPlayerModule audioPlayerModule)
        {
            Track track = audioPlayerModule.GetTracksForGuild(ctx.Guild.Id).SingleOrDefault(track => track.Name == trackName.ToLowerInvariant());

            if (track == null)
            {
                await ctx.Message.RespondAsync("I can't find the requested track.");
                return;
            }

            await audioPlayerModule.PlayTrack(ctx, track);
        }

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Tracks(CommandContext ctx, int page = 1, [FromServices] AudioPlayerModule module = null, [FromServices] IBotCoreModule botCore = null)
        {
            if (page <= 0)
            {
                await ctx.Message.RespondAsync("Please select a page number greater than 0.");
                return;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle("Tracks")
                .WithCustomFooterWithColour(ctx);

            IEnumerable<Track> tracks = module.GetTracksForGuild(ctx.Guild.Id);

            if (tracks.Count() > module.TrackPageSize)
            {
                int pageCount = (tracks.Count() / module.TrackPageSize) + ((tracks.Count() % module.TrackPageSize) == 0 ? 0 : 1);
                builder.WithDescription($"Showing page {page} of {pageCount}. Use `{botCore.CommandHandler.CommandPrefix}tracks <pageNumber>` to view more.");

                if (page == 1)
                    tracks = tracks.Take(module.TrackPageSize);
                else
                    tracks = tracks.Skip((page - 1) * module.TrackPageSize).Take(module.TrackPageSize);
            }

            if (!tracks.Any())
            {
                await ctx.Message.RespondAsync("No tracks on that page.");
                return;
            }

            foreach (Track track in tracks)
                builder.AddField(track.Name, track.Description ?? "No description provided.");

            await ctx.Message.RespondAsync(embed: builder);
            await ctx.Message.DeleteAsync();
        }
    }
}
