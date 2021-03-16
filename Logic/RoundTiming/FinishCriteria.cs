using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class FinishCriteria : IFinishCriteria
    {
        public FinishCriteria(FinishCriteriaDto dto) :
            this(dto.Duration, dto.TotalLaps, dto.LapsAfterDuration, dto.SkipStartingCheckpoint, dto.ForceFinishOnly)
        {
        }

        public FinishCriteria(TimeSpan duration, int? totalLaps, int lapsAfterDuration, bool skipStartingCheckpoint,
            bool forceFinishOnly = false)
        {
            Duration = duration;
            TotalLaps = totalLaps;
            LapsAfterDuration = lapsAfterDuration;
            SkipStartingCheckpoint = skipStartingCheckpoint;
            ForceFinishOnly = forceFinishOnly;
        }

        public TimeSpan Duration { get; }
        public int? TotalLaps { get; }
        public int LapsAfterDuration { get; }
        public bool SkipStartingCheckpoint { get; }
        public bool ForceFinishOnly { get; }

        public bool HasFinished(RoundPosition current, IEnumerable<RoundPosition> sequence, bool finishForced)
        {
            if (ForceFinishOnly && !finishForced) return false;
            if (current.Finished) return true;
            var leader = GetLeader(sequence, finishForced);
            if (current.RiderId == leader.RiderId)
            {
                if (TotalLaps.HasValue)
                {
                    var startingLap = SkipStartingCheckpoint ? 1 : 0;
                    if (current.LapCount - startingLap >= TotalLaps)
                        return true;
                    return current.LapCount > startingLap && current.Duration >= Duration;
                }

                var mainDurationComplete = current.Duration >= Duration;
                var additionalLapsComplete = LapsAfterDuration == 0 ||
                                             current.Laps.Count(x => x.AggDuration >= Duration) > LapsAfterDuration;
                return mainDurationComplete && additionalLapsComplete;
            }

            if (!leader.Finished) return false;
            return current.EndSequence > leader.EndSequence;
        }

        public static FinishCriteria FromDuration(TimeSpan duration, int lapsAfterDuration = 0)
        {
            return new(duration, null, lapsAfterDuration, false);
        }

        /// <summary>
        ///     Sets finished only with finishForced. Used for calculation without timestamps
        /// </summary>
        /// <returns></returns>
        public static FinishCriteria FromForcedFinish()
        {
            return new(TimeSpan.Zero, null, 0, false, true);
        }

        public static FinishCriteria FromTotalLaps(int totalLaps, TimeSpan duration, bool skipFirstLap = false)
        {
            return new(duration, totalLaps, 0, skipFirstLap);
        }

        public RoundPosition GetLeader(IEnumerable<RoundPosition> sequence, bool finishForced)
        {
            RoundPosition first = null;
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
                if (position.Duration >= Duration)
                    return position;
            }

            return first;
        }
    }
}