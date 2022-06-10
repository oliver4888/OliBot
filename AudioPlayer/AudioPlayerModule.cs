using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using OliBot.API;
using OliBot.API.Attributes;
using OliBot.API.Interfaces;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;

namespace AudioPlayer
{
    [Module]
    public class AudioPlayerModule
    {
        readonly ILogger<AudioPlayerModule> _logger;
        readonly IBotCoreModule _botCoreModule;

        readonly AudioPlayer _config;
        readonly VoiceNextExtension _voiceNextExtension;

        public int TrackPageSize => _config.TrackPageSize;

        public AudioPlayerModule(ILogger<AudioPlayerModule> logger, IBotCoreModule botCoreModule, AudioPlayer config)
        {
            // These dlls are imported via DllImportAttribute which does not trigger BotRunner's assembly resolve functionality
            string opus = "libopus.dll", sodium = "libsodium.dll";
            CopyNativeLib(opus);
            CopyNativeLib(sodium);

            _logger = logger;
            _botCoreModule = botCoreModule;
            _botCoreModule.CommandHandler.RegisterCommands<AudioPlayerCommands>();
            _botCoreModule.DiscordClient.VoiceStateUpdated += VoiceStateUpdated;

            _config = config;
            _voiceNextExtension = _botCoreModule.DiscordClient.UseVoiceNext(new VoiceNextConfiguration { EnableIncoming = false });

            foreach (Track track in config.Tracks)
                if (track.FileName != null && track.FileNames != null)
                    _logger.LogWarning(
                        "Track \"{trackName}\" has a file name and a list of file names, if the file name is not also included in the list it will not be used as a trigger!",
                        track.Name);
        }

        private void CopyNativeLib(string libName)
        {
            string currentPath = Directory.GetCurrentDirectory();
            string modulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentPath != modulePath)
                File.Copy(Path.Combine(modulePath, "native-libs", libName), Path.Combine(currentPath, libName), true);
        }

        private async Task VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            VoiceNextConnection vnc = _voiceNextExtension.GetConnection(e.Guild);

            if (vnc == null)
                return;

            if (e.User.Id == client.CurrentUser.Id)
            {
                if (!e.After.IsServerDeafened)
                {
                    DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
                    await member.ModifyAsync(member => member.Deafened = true);
                }
            }
            else
            {
                if (e.Before?.Channel != null && e.Before.Channel.Id == vnc.TargetChannel.Id && e.Before.Channel.Users.Where(u => !u.IsBot).Count() == 0)
                    vnc.Disconnect();
            }
        }

        public IEnumerable<Track> GetTracksForGuild(ulong guildId) =>
            _config.Tracks.Where(track => track.GuildIdWhitelist == null || track.GuildIdWhitelist.Contains(guildId));

        public async Task<bool> TryJoinVoiceChannel(CommandContext ctx) => await TryJoinVoiceChannel(ctx.Guild, ctx.Member, ctx.Message);

        public async Task<bool> TryJoinVoiceChannel(DiscordGuild guild, DiscordMember commandAuthor, DiscordMessage commandMessage)
        {
            VoiceNextConnection vnc = _voiceNextExtension.GetConnection(guild);

            DiscordChannel chn = commandAuthor?.VoiceState?.Channel;

            if (chn == null)
            {
                await commandMessage.RespondAsync("You need to be in a voice channel!");
                return false;
            }

            if (guild.AfkChannel != null && guild.AfkChannel.Id == chn.Id)
            {
                await commandMessage.RespondAsync("You are in the AFK channel!");
                return false;
            }

            if (vnc != null)
            {
                if (vnc.TargetChannel.Id == chn.Id)
                    await commandMessage.RespondAsync("I am already in that channel!");
                else
                    await commandMessage.RespondAsync("I am already in a channel for this guild!");
                return false;
            }

            await chn.ConnectAsync();

            DiscordMember bot = await guild.GetMemberAsync(_botCoreModule.DiscordClient.CurrentUser.Id);
            if (!bot.VoiceState.IsServerDeafened)
                await bot.ModifyAsync(member => member.Deafened = true);

            return true;
        }

        public async Task PlayTrack(CommandContext ctx, Track track)
        {
            VoiceNextConnection vnc = _voiceNextExtension.GetConnection(ctx.Guild);
            bool isConnected = vnc != null;

            if (vnc != null && vnc.IsPlaying)
            {
                await ctx.Message.RespondAsync("I am already playing something!");
                return;
            }

            if (isConnected)
            {
                DiscordChannel chn = ctx.Member.VoiceState?.Channel;
                if (chn == null)
                {
                    await ctx.Message.RespondAsync("You need to be in a voice channel!");
                    return;
                }
                else if (ctx.Guild.AfkChannel != null && ctx.Guild.AfkChannel.Id == chn.Id)
                {
                    await ctx.Message.RespondAsync("You are in the AFK channel!");
                    return;
                }
                else if (vnc.TargetChannel.Id != chn.Id)
                {
                    await ctx.Message.RespondAsync("I am not in your channel!");
                    return;
                }
            }

            if (isConnected || await TryJoinVoiceChannel(ctx))
            {
                if (!isConnected)
                    vnc = _voiceNextExtension.GetConnection(ctx.Guild);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = _config.FfmpegLocation,
                    Arguments = $@"-i ""{Path.Combine(_config.AudioFolderLocation, track.GetFileName())}"" -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process ffmpeg = Process.Start(psi);
                Stream ffout = ffmpeg.StandardOutput.BaseStream;

                VoiceTransmitSink txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
            }
        }
    }
}
