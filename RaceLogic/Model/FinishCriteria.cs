using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceLogic.Model
{
    public class FinishCriteria : IFinishCriteria
    {
        private readonly TimeSpan duration;
        private readonly int? totalLaps;
        private readonly int lapsAfterDuration;
        private readonly bool skipStartingCheckpoint;
        private readonly bool forceFinishOnly;

        private FinishCriteria(TimeSpan duration, int? totalLaps, int lapsAfterDuration, bool skipStartingCheckpoint, bool forceFinishOnly = false)
        {
            this.duration = duration;
            this.totalLaps = totalLaps;
            this.lapsAfterDuration = lapsAfterDuration;
            this.skipStartingCheckpoint = skipStartingCheckpoint;
            this.forceFinishOnly = forceFinishOnly;
        }

        public static FinishCriteria FromDuration(TimeSpan duration, int lapsAfterDuration = 0)
        {
            return new FinishCriteria(duration, null, lapsAfterDuration, false);
        }
        
        /// <summary>
        /// Sets finished only with finishForced. Used for calculation without timestamps
        /// </summary>
        /// <returns></returns>
        public static FinishCriteria FromForcedFinish()
        {
            return new FinishCriteria(TimeSpan.Zero, null, 0, false, true);
        }
        
        public static FinishCriteria FromTotalLaps(int totalLaps, TimeSpan duration, bool skipFirstLap = false)
        {
            return new FinishCriteria(duration, totalLaps, 0, skipFirstLap);
        }
        
        public bool HasFinished<TRiderId>(RoundPosition<TRiderId> current, IEnumerable<RoundPosition<TRiderId>> sequence, bool finishForced)
            where TRiderId: IEquatable<TRiderId>
        {
            if (forceFinishOnly && !finishForced) return false;
            if (current.Finished) return true;
            var leader = GetLeader(sequence, finishForced);
            if (current.RiderId.Equals(leader.RiderId))
            {
                if (totalLaps.HasValue)
                {
                    var startingLap = skipStartingCheckpoint ? 1 : 0;
                    if (current.LapsCount - startingLap >= totalLaps)
                        return true;
                    return current.LapsCount > startingLap && current.Duration >= duration;
                }
                else
                {
                    var mainDurationComplete = current.Duration >= duration;
                    var additionalLapsComplete = lapsAfterDuration == 0 || current.Laps.Count(x => x.AggDuration >= duration) > lapsAfterDuration;
                    return mainDurationComplete && additionalLapsComplete;
                }
            }
            if (!leader.Finished) return false;
            return current.EndSequence > leader.EndSequence;
        }
        
        public RoundPosition<TRiderId> GetLeader<TRiderId>(IEnumerable<RoundPosition<TRiderId>> sequence, bool finishForced)
            where TRiderId: IEquatable<TRiderId>
        {
            RoundPosition<TRiderId> first = null;
            foreach (var position in sequence)
            {
                if (!finishForced || position.Finished)
                    return position;
                // The sequence is ordered by lap count and checkpoint sequence
                // Finish may have to be forced if the leader by laps, have fallen from race
                // and will not going to finish. When finish is forced, we look for
                // the first rider who have completed main time and chose him as leader
                if (first == null)
                    first = position;
                if (position.Duration >= duration)
                    return position;
            }
            return first;
        }
    }
}