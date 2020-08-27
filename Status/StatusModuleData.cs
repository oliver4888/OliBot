using DSharpPlus.Entities;
using System.Collections.Generic;

namespace Status
{
    public class StatusModuleConfig
    {
        public StatusMode Mode { get; set; }
        public IList<Status> Statuses { get; set; } = new List<Status>();
    }

    public class Status
    {
        public ActivityType ActivityType { get; set; }
        public string Message { get; set; }
    }

    public enum StatusMode
    {
        Automatic,
        Manual
    }
}
