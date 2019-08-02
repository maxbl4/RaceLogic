using System;
using maxbl4.RaceLogic.Sessions;

namespace maxbl4.RaceLogic.LogManagement.EntryTypes
{
    public class SessionStart: Entry, ISessionConfiguration
    {
        public SessionStartTypes StartType { get; set; }
        public TimeSpan Duration { get; set; }
        public int? TotalLaps { get; set; }
        public int LapsAfterDuration { get; set; }
        public bool SkipStartingCheckpoint { get; set; }
        public bool ForceFinishOnly { get; set; }
        public TimeSpan MinimalLap { get; set; }
    }
}