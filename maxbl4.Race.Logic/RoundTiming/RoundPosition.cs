using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class RoundPosition
    {
        public int LapsCount { get; }
        public ReadOnlyCollection<Lap> Laps { get; }
        public TimeSpan Duration { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public bool Finished { get; }
        public bool Started => LapsCount > 0;
        public string RiderId { get; }
        public long StartSequence { get; }
        public long EndSequence { get; }
        
        private RoundPosition(string riderId, bool finished, DateTime? start = null, IEnumerable<Lap> laps = null)
        {
            RiderId = riderId;
            Start = start ?? default;
            Laps = new ReadOnlyCollection<Lap>(laps?.ToList() ?? new List<Lap>());
            LapsCount = Laps.Count;
            if (Start == default && Laps.Count > 0 && Laps[0].Start != default)
                Start = Laps[0].Start;
            End = Laps.LastOrDefault()?.End ?? default;
            Duration = End - Start;
            Finished = finished;
            StartSequence = Laps.FirstOrDefault()?.Checkpoint.Id ?? 0;
            EndSequence = Laps.LastOrDefault()?.Checkpoint.Id ?? 0;
        }
        
        public static RoundPosition FromStartTime(string riderId, DateTime? roundStartTime = null)
        {
            return new RoundPosition(riderId, false, roundStartTime);
        }
        
        public static RoundPosition FromLaps(string riderId, IEnumerable<Lap> laps, bool finished)
        {
            return new RoundPosition(riderId, finished, null, laps);
        }

        public RoundPosition Append(Checkpoint cp, bool finish = false)
        {
            if (RiderId != cp.RiderId)
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            
            var newLaps = Laps.Concat(new []{Laps.LastOrDefault()?.CreateNext(cp) ?? new Lap(cp, Start)});
            return FromLaps(RiderId, newLaps, finish);
        }

        public RoundPosition Finish()
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