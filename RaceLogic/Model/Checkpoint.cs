using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using RaceLogic.Extensions;

namespace RaceLogic.Model
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

        readonly Dictionary<string, int> histogram;

        public string Histogram { get; }

        private AggCheckpoint(TRiderId riderId, DateTime timestamp, DateTime lastSeen, 
            int count, Dictionary<string,int> histogram) 
            : base(riderId, timestamp)
        {
            LastSeen = lastSeen;
            Count = count;
            this.histogram = histogram;
            Histogram = ToHistogramString(this.histogram);
        }
        
        private AggCheckpoint(TRiderId riderId, DateTime timestamp, DateTime lastSeen, 
            int count, IEnumerable<KeyValuePair<string,int>> histogram = null) 
                : base(riderId, timestamp)
        {
            LastSeen = lastSeen;
            Count = count;
            this.histogram = histogram?
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
            Histogram = ToHistogramString(this.histogram);
        }

        public static AggCheckpoint<TRiderId> From(Checkpoint<TRiderId> checkpoint)
        {
            return From(new []{checkpoint});
        }
        
        public static AggCheckpoint<TRiderId> From(IEnumerable<Checkpoint<TRiderId>> checkpoints)
        {
            var riderId = default(TRiderId);
            var timestamp = default(DateTime);
            var lastSeen = default(DateTime);
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
                timestamp = timestamp.TakeSmaller(cp.Timestamp);
                lastSeen = lastSeen.TakeLarger(cp.Timestamp);
                histogram.UpdateOrAdd(cp.GetType().Name, x => x + 1);
            }
            if (count == 0)
                return new AggCheckpoint<TRiderId>(default(TRiderId), 
                    default(DateTime), default(DateTime), 
                    0);
            return new AggCheckpoint<TRiderId>(riderId, timestamp, lastSeen, count, histogram);
        }

        public AggCheckpoint<TRiderId> Add(Checkpoint<TRiderId> cp)
        {
            if (!RiderId.Equals(cp.RiderId))
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            var record = new []{new KeyValuePair<string, int>(cp.GetType().Name, 1)};
            
            return new AggCheckpoint<TRiderId>(RiderId, 
                Timestamp.TakeSmaller(cp.Timestamp),
                LastSeen.TakeLarger(cp.Timestamp),
                Count + 1, histogram?.Concat(record) ?? record);
        }

        string ToHistogramString(IDictionary<string, int> h)
        {
            return h == null ? string.Empty
                : string.Join(", ", h.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value}"));
        }
    }
}