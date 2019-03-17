using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RaceLogic.Model
{
    public class RoundPosition<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        public int Points { get; set; }
        public int Position { get; set; }
        public int LapsCount { get; }
        public ReadOnlyCollection<Lap<TRiderId>> Laps { get; }
        public TimeSpan Duration { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public bool Finished { get; }
        public bool Started => LapsCount > 0;
        public TRiderId RiderId { get; }
        
        private RoundPosition(TRiderId riderId, int lapsCount, bool finished)
        {
            RiderId = riderId;
            LapsCount = lapsCount;
            Laps = new ReadOnlyCollection<Lap<TRiderId>>(new List<Lap<TRiderId>>());
            Finished = finished;
        }
        
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
        }
        
        public static RoundPosition<TRiderId> FromLapCount(TRiderId riderId, int lapCount, bool finished)
        {
            return new RoundPosition<TRiderId>(riderId, lapCount, finished);
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
            if (Laps.Count == 0)
                return FromLapCount(RiderId, LapsCount, true);
            return FromLaps(RiderId, Laps, true);
        }
        
        public override string ToString()
        {
            return $"{RiderId} L:{LapsCount}";
        }
    }
}