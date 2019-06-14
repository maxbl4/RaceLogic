using System;
using System.Collections.Generic;
using System.Threading;

namespace maxbl4.RaceLogic.Checkpoints
{
    public class Checkpoint<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        private static long nextSequence;
        public DateTime Timestamp { get; }
        public TRiderId RiderId { get; }
        public bool HasTimestamp => Timestamp > default(DateTime);
        public long Sequence { get; }

        public Checkpoint(TRiderId riderId, DateTime? timestamp = null)
        {
            Sequence = Interlocked.Increment(ref nextSequence);
            RiderId = riderId;
            Timestamp = timestamp ?? default(DateTime);
        }

        public override string ToString()
        {
            return $"{RiderId} Ts:{Timestamp:t}";
        }
        
        private sealed class TimestampRelationalComparer : IComparer<Checkpoint<TRiderId>>
        {
            public int Compare(Checkpoint<TRiderId> x, Checkpoint<TRiderId> y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }
        
        public static IComparer<Checkpoint<TRiderId>> TimestampComparer { get; } = new TimestampRelationalComparer();
    }
}