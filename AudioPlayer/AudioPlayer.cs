using Common;
using Common.Attributes;

namespace AudioPlayer
{
    [DependencyInjected(DIType.Options)]
    public class AudioPlayer
    {
        public string FfmpegLocation { get; set; }
        public string AudioFolderLocation { get; set; }
    }
}
