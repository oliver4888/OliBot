﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using AudioPlayer.Extensions;

using Common;
using Common.Attributes;
using Common.Extensions;
using Common.Interfaces;

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
            if (ctx.Member.VoiceState.Channel == null)
                await ctx.Message.RespondAsync("You are not in a voice channel!");
            else if (vnc.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                await ctx.Message.RespondAsync("I am not in your channel!");
            else if (vnc != null)
                vnc.Disconnect();
        }

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Play(CommandContext ctx, [RemainingText] string trackName, [FromServices] AudioPlayerModule audioPlayerModule, [FromServices] AudioPlayer config)
        {
            Track track = config.Tracks.SingleOrDefault(track => track.Name == trackName.ToLowerInvariant());

            if (track == null)
            {
                await ctx.Message.RespondAsync("I can't find the requested track.");
                return;
            }

            await audioPlayerModule.PlayTrack(ctx, track);
        }

        [Command(disableDMs: true, groupName: "Audio Player")]
        public async Task Tracks(CommandContext ctx, int page = 1, [FromServices] AudioPlayer config = null, [FromServices] IBotCoreModule botCore = null)
        {
            if (page == 0)
            {
                await ctx.Message.RespondAsync("Please select a page number greater than 0.");
                return;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle("Tracks")
                .WithCustomFooterWithColour(ctx);

            IEnumerable<Track> tracks = config.Tracks;

            if (config.Tracks.Count() > config.TrackPageSize)
            {
                if (page == 1)
                    tracks = tracks.Take(config.TrackPageSize);
                else
                    tracks = tracks.Skip((page - 1) * config.TrackPageSize).Take(config.TrackPageSize);

                int pageCount = (config.Tracks.Count() / config.TrackPageSize) + ((config.Tracks.Count() % config.TrackPageSize) == 0 ? 0 : 1);
                builder.WithDescription($"Showing page {page} of {pageCount} use `{botCore.CommandHandler.CommandPrefix}tracks <pageNumber>` to view more.");
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
