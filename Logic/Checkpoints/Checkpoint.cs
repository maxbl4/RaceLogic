using System;
using System.Collections.Generic;
using System.Threading;

namespace maxbl4.Race.Logic.Checkpoints
{
    public class Checkpoint : ICheckpoint
    {
        private static long nextSequence;
        public DateTime Timestamp { get; set; } = Constants.DefaultUtcDate;
        public string RiderId { get; set; }
        public long Id { get; set; }
        
        public DateTime LastSeen { get; set; } = Constants.DefaultUtcDate;
        public int Count { get; set; } = 1;
        public bool Aggregated { get; set; }
        public bool IsManual { get; set; }
        public int Rps { get; set; }

        public Checkpoint()
        {
        }

        public Checkpoint(string riderId, DateTime? timestamp = null)
        {
            Id = Interlocked.Increment(ref nextSequence);
            RiderId = riderId;
            LastSeen = Timestamp = timestamp ?? Constants.DefaultUtcDate;
        }

        public override string ToString()
        {
            return $"{RiderId} Ts:{Timestamp:t}";
        }

        public Checkpoint WithRiderId(string riderId)
        {
            return new Checkpoint
            {
                RiderId = riderId,
                Aggregated = Aggregated,
                Count = Count,
                Id = Id,
                Rps = Rps,
                Timestamp = Timestamp,
                IsManual = IsManual,
                LastSeen = LastSeen
            };
        }
        
        private sealed class TimestampRelationalComparer : IComparer<Checkpoint>
        {
            public int Compare(Checkpoint x, Checkpoint y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }
        
        public static IComparer<Checkpoint> TimestampComparer { get; } = new TimestampRelationalComparer();
    }
}