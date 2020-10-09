using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Common;
using Common.Attributes;
using Common.Interfaces;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

namespace AudioPlayer
{
    [Module]
    public class AudioPlayerModule
    {
        static IBotCoreModule _botCoreModule;
        static AudioPlayer _config;

        static VoiceNextExtension _voiceNextExtension;

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

        private async Task VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            VoiceNextConnection vnc = _voiceNextExtension.GetConnection(e.Guild);

            if (vnc == null)
                return;

            if (e.User.Id == e.Client.CurrentUser.Id)
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

        public static async Task<bool> TryJoinVoiceChannel(CommandContext ctx) => await TryJoinVoiceChannel(ctx.Guild, ctx.Member, ctx.Message);

        public static async Task<bool> TryJoinVoiceChannel(DiscordGuild guild, DiscordMember commandAuthor, DiscordMessage commandMessage)
        {
            VoiceNextConnection vnc = _voiceNextExtension.GetConnection(guild);

            DiscordChannel chn = commandAuthor?.VoiceState?.Channel;

            if (chn == null)
            {
                await commandMessage.RespondAsync("You need to be in a voice channel!");
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
    }
}
