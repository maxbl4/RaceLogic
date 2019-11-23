using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.RaceLogic.Extensions;

namespace maxbl4.RaceLogic.Checkpoints
{
    public class AggCheckpoint : Checkpoint
    {
        readonly Dictionary<string, int> histogram;

        public string Histogram { get; set; }

        public AggCheckpoint()
        {
        }

        public AggCheckpoint(string riderId, DateTime timestamp, DateTime lastSeen, 
            int count, Dictionary<string,int> histogram) 
            : base(riderId, timestamp)
        {
            Aggregated = true;
            LastSeen = lastSeen;
            Count = count;
            this.histogram = histogram;
            Histogram = ToHistogramString(this.histogram);
        }
        
        public AggCheckpoint(string riderId, DateTime timestamp, DateTime lastSeen, 
            int count, IEnumerable<KeyValuePair<string,int>> histogram = null) 
                : base(riderId, timestamp)
        {
            Aggregated = true;
            LastSeen = lastSeen;
            Count = count;
            this.histogram = histogram?
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
            Histogram = ToHistogramString(this.histogram);
        }

        public static AggCheckpoint From(Checkpoint checkpoint)
        {
            return From(new []{checkpoint});
        }
        
        public static AggCheckpoint From(IEnumerable<Checkpoint> checkpoints)
        {
            var riderId = default(string);
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
                else if (cp.RiderId != riderId)
                {
                    throw new ArgumentException($"Found checkpoints with different RiderIds {riderId} {cp.RiderId}", nameof(checkpoints));
                }
                timestamp = timestamp.TakeSmaller(cp.Timestamp);
                lastSeen = lastSeen.TakeLarger(cp.Timestamp);
                histogram.UpdateOrAdd(cp.GetType().Name, x => x + 1);
            }
            if (count == 0)
                return new AggCheckpoint(default, 
                    default, default, 
                    0);
            return new AggCheckpoint(riderId, timestamp, lastSeen, count, histogram);
        }

        public AggCheckpoint Add(Checkpoint cp)
        {
            if (RiderId != cp.RiderId)
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            var record = new []{new KeyValuePair<string, int>(cp.GetType().Name, 1)};
            
            return new AggCheckpoint(RiderId, 
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