using System;
using System.Linq;
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
        public IReadOnlyCollection<Track> Tracks { get; set; }
    }

    public class Track
    {
        readonly Random _random = new Random();

        public string Name { get; set; }
        public string FileName { get; set; }
        public IReadOnlyCollection<string> FileNames { get; set; }
        public string Description { get; set; }
        public IReadOnlyCollection<ulong> GuildIdWhitelist { get; set; }

        public string GetFileName() => FileNames == null ? FileName : FileNames.ToArray()[_random.Next(0, FileNames.Count())];
    }
}
