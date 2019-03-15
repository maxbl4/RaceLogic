using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;

namespace RaceLogic.Model
{
    public class Checkpoint<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        public DateTime Timestamp { get; }
        public TRiderId RiderId { get; }
        public bool HasTimestamp => Timestamp > DateTime.MinValue;
        
        public Checkpoint(TRiderId riderId, DateTime? timestamp = null)
        {
            RiderId = riderId;
            Timestamp = timestamp ?? DateTime.MinValue;
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

    public class RfidCheckpoint<TRiderId> : Checkpoint<TRiderId>
        where TRiderId : IEquatable<TRiderId>
    {
        public string TagId { get; }

        public RfidCheckpoint(TRiderId riderId, DateTime timestamp, string tagId) : base(riderId, timestamp)
        {
            TagId = tagId;
        }
    }
    
    public class AggCheckpoint<TRiderId> : Checkpoint<TRiderId>
        where TRiderId : IEquatable<TRiderId>
    {
        public DateTime LastSeen { get; }
        public int Count { get; }
        public bool IsEmpty => Count < 1;
        public string Histogram { get; }

        private AggCheckpoint(TRiderId riderId, DateTime timestamp, DateTime lastSeen, 
            int count, string histogram) 
                : base(riderId, timestamp)
        {
            LastSeen = lastSeen;
            Count = count;
            Histogram = histogram;
        }

        public static AggCheckpoint<TRiderId> From(IEnumerable<Checkpoint<TRiderId>> checkpoints)
        {
            var riderId = default(TRiderId);
            var timestamp = DateTime.MinValue;
            var lastSeen = DateTime.MinValue;
            var count = 0;
            var histogram = new Dictionary<string, int>();
            foreach (var cp in checkpoints)
            {
                count++;
                if (count == 1)
                {
                    riderId = cp.RiderId;
                    timestamp = lastSeen = cp.Timestamp;
                }
                else if (!riderId.Equals(cp.RiderId))
                {
                    throw new ArgumentException($"Found checkpoints with different RiderIds {riderId} {cp.RiderId}", nameof(checkpoints));
                }

                if (timestamp > cp.Timestamp)
                    timestamp = cp.Timestamp;
                if (lastSeen < cp.Timestamp)
                    lastSeen = cp.Timestamp;
                histogram.UpdateOrAdd(cp.GetType().GetGenericTypeDefinition().Name, x => x + 1);
            }
            if (count == 0)
                return new AggCheckpoint<TRiderId>(default(TRiderId), 
                    DateTime.MinValue, DateTime.MinValue, 
                    0, string.Empty);
            return new AggCheckpoint<TRiderId>(riderId, timestamp, lastSeen, count,
                string.Join(", ", histogram.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value}")));
        }
    }
}