using System;

namespace maxbl4.Race.Logic.RoundTiming
{
    public static class TrackOfCheckpointsFactory
    {
        public static Func<DateTime?, IFinishCriteria, ITrackOfCheckpoints> Create = 
            (roundStartTime, finishCriteria) => new TrackOfCheckpointsIncrementalCustomSort(roundStartTime, finishCriteria);
    }
}