using System.Collections.Generic;

using Common;
using Common.Attributes;

namespace AudioPlayer
{
    [DependencyInjected(DIType.Options)]
    public class AudioPlayer
    {
        public string FfmpegLocation { get; set; }
        public string AudioFolderLocation { get; set; }
        public int TrackPageSize { get; set; }
        public IReadOnlyList<Track> Tracks { get; set; }
    }

    public class Track
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
    }
}
