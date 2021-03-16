using System;
using System.Collections.Generic;

namespace maxbl4.Race.Logic.RoundTiming
{
    public interface IFinishCriteria
    {
        TimeSpan Duration { get; }
        int? TotalLaps { get; }
        int LapsAfterDuration { get; }
        bool SkipStartingCheckpoint { get; }
        bool ForceFinishOnly { get; }
        bool HasFinished(RoundPosition current, IEnumerable<RoundPosition> sequence, bool finishForced);
    }
}