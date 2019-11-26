using System;
using System.Collections.Generic;
using System.Threading;

namespace maxbl4.RaceLogic.Checkpoints
{
    public class Checkpoint : ICheckpoint
    {
        private static long nextSequence;
        public DateTime Timestamp { get; set; } = new DateTime(0, DateTimeKind.Utc);
        public string RiderId { get; set; }
        public long Id { get; set; }
        
        public DateTime LastSeen { get; set; } = new DateTime(0, DateTimeKind.Utc);
        public int Count { get; set; } = 1;
        public bool Aggregated { get; set; }
        public bool IsManual { get; set; }
        public double Rps { get; set; }

        public Checkpoint()
        {
        }

        public Checkpoint(string riderId, DateTime? timestamp = null)
        {
            Id = Interlocked.Increment(ref nextSequence);
            RiderId = riderId;
            LastSeen = Timestamp = timestamp ?? new DateTime(0, DateTimeKind.Utc);
        }

        public override string ToString()
        {
            return $"{RiderId} Ts:{Timestamp:t}";
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