﻿using Common;
using Common.Attributes;

namespace SteamHelper
{
    [DependencyInjected(DIType.Options)]
    public class SteamHelper
    {
        public string Token { get; set; }
        public int SlidingExpirationHours { get; set; }
        public string FileCachePath { get; set; }
    }

}
