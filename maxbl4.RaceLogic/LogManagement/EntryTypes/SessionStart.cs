using System;

namespace maxbl4.RaceLogic.LogManagement.EntryTypes
{
    public class SessionStart: Entry
    {
        public TimeSpan Duration { get; set; }
        public int TotalLaps { get; set; }
        public int LapsAfterDuration{ get; set; }
        public bool SkipStartingCheckpoint { get; set; }
        public bool ForceFinishOnly { get; set; }
    }
}