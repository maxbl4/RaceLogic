using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class RoundPosition
    {
        private sealed class LapsCountFinishedRelationalComparer : IComparer<RoundPosition>
        {
            public int Compare(RoundPosition x, RoundPosition y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                var finishedComparison = x.Finished.CompareTo(y.Finished);
                if (finishedComparison != 0) return finishedComparison * -1;
                return x.LapsCount.CompareTo(y.LapsCount) * -1;
            }
        }

        public static IComparer<RoundPosition> LapsCountFinishedComparer { get; } = new LapsCountFinishedRelationalComparer();

        public int LapsCount { get; private set; }
        public ReadOnlyCollection<Lap> Laps { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public bool Finished { get; private set; }
        public bool Started => LapsCount > 0;
        public string RiderId { get; private set; }
        public long StartSequence { get; private set; }
        public long EndSequence { get; private set; }

        private RoundPosition()
        {
        }

        private RoundPosition Update(string riderId, bool finished, DateTime? start = null, IEnumerable<Lap> laps = null)
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
            return this;
        }
        
        public static RoundPosition FromStartTime(string riderId, DateTime? roundStartTime = null)
        {
            return new RoundPosition().Update(riderId, false, roundStartTime);
        }
        
        public static RoundPosition FromLaps(string riderId, IEnumerable<Lap> laps, bool finished)
        {
            return new RoundPosition().Update(riderId, finished, null, laps);
        }
        
        private void UpdateFromLaps(string riderId, IEnumerable<Lap> laps, bool finished)
        {
            Update(riderId, finished, null, laps);
        }

        public void Append(Checkpoint cp, bool finish = false)
        {
            if (RiderId != cp.RiderId)
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            
            var newLaps = Laps.Concat(new []{Laps.LastOrDefault()?.CreateNext(cp) ?? new Lap(cp, Start)});
            UpdateFromLaps(RiderId, newLaps, finish);
        }

        public void Finish()
        {
            if (LapsCount == 0)
                return;
            UpdateFromLaps(RiderId, Laps, true);
        }
        
        public override string ToString()
        {
            return $"{(Finished?"F":"")}{RiderId} L:{LapsCount}";
        }
    }
}