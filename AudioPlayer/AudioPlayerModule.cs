using System.IO;
using System.Reflection;

using Common.Attributes;
using Common.Interfaces;

using DSharpPlus.VoiceNext;

namespace AudioPlayer
{
    [Module]
    public class AudioPlayerModule
    {
        readonly IBotCoreModule _botCoreModule;
        readonly VoiceNextExtension _voiceNextExtension;

        public AudioPlayerModule(IBotCoreModule botCoreModule)
        {
            // These dlls are imported via DllImportAttribute which does not trigger BotRunner's assembly resolve functionality
            string opus = "libopus.dll", sodium = "libsodium.dll";
            CopyNativeLib(opus);
            CopyNativeLib(sodium);

            _botCoreModule = botCoreModule;
            _botCoreModule.CommandHandler.RegisterCommands<AudioPlayerCommands>();
            _voiceNextExtension = _botCoreModule.DiscordClient.UseVoiceNext(new VoiceNextConfiguration { EnableIncoming = false });
        }

        public void CopyNativeLib(string libName)
        {
            string currentPath = Directory.GetCurrentDirectory();
            string modulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentPath != modulePath)
                File.Copy(Path.Combine(modulePath, "native-libs", libName), Path.Combine(currentPath, libName), true);
        }
    }
}
