using System;
using maxbl4.Race.Logic.Sessions;

namespace maxbl4.Race.Logic.LogManagement.EntryTypes
{
    public class SessionStart: IEntry, ISessionConfiguration
    {
        public SessionStartTypes StartType { get; set; }
        public TimeSpan Duration { get; set; }
        public int? TotalLaps { get; set; }
        public int LapsAfterDuration { get; set; }
        public bool SkipStartingCheckpoint { get; set; }
        public bool ForceFinishOnly { get; set; }
        public TimeSpan MinimalLap { get; set; }
        public DateTime Timestamp { get; set; }
        public long Id { get; set; }
    }
}