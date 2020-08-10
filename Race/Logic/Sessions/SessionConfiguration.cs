using System;

namespace maxbl4.Race.Logic.Sessions
{
    public interface ISessionConfiguration
    {
        SessionStartTypes StartType { get; set; }
        TimeSpan Duration { get; set; }
        int? TotalLaps { get; set; }
        int LapsAfterDuration { get; set; }
        bool SkipStartingCheckpoint { get; set; }
        bool ForceFinishOnly { get; set; }
        TimeSpan MinimalLap { get; set; }
    }

    public class SessionConfiguration : ISessionConfiguration
    {
        public SessionStartTypes StartType { get; set; }
        public TimeSpan Duration { get; set; }
        public int? TotalLaps { get; set; }
        public int LapsAfterDuration { get; set; }
        public bool SkipStartingCheckpoint { get; set; }
        public bool ForceFinishOnly { get; set; }

        public TimeSpan MinimalLap { get; set; }
    }

    public enum SessionStartTypes
    {
        MassStart,
        IndividualStart //TODO: support individual start in track of checkpoints
    }
}