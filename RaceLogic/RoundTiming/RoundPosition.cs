using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RaceLogic.Checkpoints;

namespace RaceLogic.RoundTiming
{
    public class RoundPosition<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        public int LapsCount { get; }
        public ReadOnlyCollection<Lap<TRiderId>> Laps { get; }
        public TimeSpan Duration { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public bool Finished { get; }
        public bool Started => LapsCount > 0;
        public TRiderId RiderId { get; }
        public long StartSequence { get; }
        public long EndSequence { get; }
        
        private RoundPosition(TRiderId riderId, bool finished, DateTime? start = null, IEnumerable<Lap<TRiderId>> laps = null)
        {
            RiderId = riderId;
            Start = start ?? default(DateTime);
            Laps = new ReadOnlyCollection<Lap<TRiderId>>(laps?.ToList() ?? new List<Lap<TRiderId>>());
            LapsCount = Laps.Count;
            if (Start == default(DateTime) && Laps.Count > 0 && Laps[0].Start != default(DateTime))
                Start = Laps[0].Start;
            End = Laps.LastOrDefault()?.End ?? default(DateTime);
            Duration = End - Start;
            Finished = finished;
            StartSequence = Laps.FirstOrDefault()?.Checkpoint.Sequence ?? 0;
            EndSequence = Laps.LastOrDefault()?.Checkpoint.Sequence ?? 0;
        }
        
        public static RoundPosition<TRiderId> FromStartTime(TRiderId riderId, DateTime? roundStartTime = null)
        {
            return new RoundPosition<TRiderId>(riderId, false, roundStartTime);
        }
        
        public static RoundPosition<TRiderId> FromLaps(TRiderId riderId, IEnumerable<Lap<TRiderId>> laps, bool finished)
        {
            return new RoundPosition<TRiderId>(riderId, finished, null, laps);
        }

        public RoundPosition<TRiderId> Append(Checkpoint<TRiderId> cp, bool finish = false)
        {
            if (!RiderId.Equals(cp.RiderId))
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            
            var newLaps = Laps.Concat(new []{Laps.LastOrDefault()?.CreateNext(cp) ?? new Lap<TRiderId>(cp, Start)});
            return FromLaps(RiderId, newLaps, finish);
        }

        public RoundPosition<TRiderId> Finish()
        {
            if (LapsCount == 0)
                return this;
            return FromLaps(RiderId, Laps, true);
        }
        
        public override string ToString()
        {
            return $"{(Finished?"F":"")}{RiderId} L:{LapsCount}";
        }
    }
}