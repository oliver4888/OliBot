using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

using Common;
using Common.Attributes;
using Common.Interfaces;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

namespace AudioPlayer
{
    [Module]
    public class AudioPlayerModule
    {
        readonly IBotCoreModule _botCoreModule;

        readonly AudioPlayer _config;
        readonly VoiceNextExtension _voiceNextExtension;

        public AudioPlayerModule(IBotCoreModule botCoreModule, AudioPlayer config)
        {
            // These dlls are imported via DllImportAttribute which does not trigger BotRunner's assembly resolve functionality
            string opus = "libopus.dll", sodium = "libsodium.dll";
            CopyNativeLib(opus);
            CopyNativeLib(sodium);

            _botCoreModule = botCoreModule;
            _botCoreModule.CommandHandler.RegisterCommands<AudioPlayerCommands>();
            _botCoreModule.DiscordClient.VoiceStateUpdated += VoiceStateUpdated;

            _config = config;
            _voiceNextExtension = _botCoreModule.DiscordClient.UseVoiceNext(new VoiceNextConfiguration { EnableIncoming = false });
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
                if (e.Before?.Channel != null && e.Before.Channel.Id == vnc.Channel.Id && e.Before.Channel.Users.Where(u => !u.IsBot).Count() == 0)
                    vnc.Disconnect();
            }
        }

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
                if (vnc.Channel.Id == chn.Id)
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

            if (isConnected || await TryJoinVoiceChannel(ctx))
            {
                if (!isConnected)
                    vnc = _voiceNextExtension.GetConnection(ctx.Guild);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = _config.FfmpegLocation,
                    Arguments = $@"-i ""{Path.Combine(_config.AudioFolderLocation, track.FileName)}"" -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process ffmpeg = Process.Start(psi);
                Stream ffout = ffmpeg.StandardOutput.BaseStream;

                VoiceTransmitStream txStream = vnc.GetTransmitStream();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
            }
        }
    }
}
