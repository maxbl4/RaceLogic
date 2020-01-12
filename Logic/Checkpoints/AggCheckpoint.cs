using System;
using System.Collections.Generic;
using maxbl4.Infrastructure.Extensions.DateTimeExt;

namespace maxbl4.Race.Logic.Checkpoints
{
    public class AggCheckpoint : Checkpoint
    {
        public AggCheckpoint()
        {
        }
        
        public AggCheckpoint(string riderId, DateTime timestamp, DateTime lastSeen, 
            int count, bool isManual) 
                : base(riderId, timestamp)
        {
            Aggregated = true;
            IsManual = isManual;
            LastSeen = lastSeen;
            Count = count;
            var interval = (lastSeen - timestamp).TotalMilliseconds;
            if (interval < 1)
                Rps = Count;
            else
            {
                Rps = (int)Math.Ceiling(Count * 1000 / interval);
            }
        }

        public static AggCheckpoint From(Checkpoint checkpoint)
        {
            return From(new []{checkpoint});
        }
        
        public static AggCheckpoint From(IEnumerable<Checkpoint> checkpoints)
        {
            var riderId = default(string);
            var timestamp = Constants.DefaultUtcDate;
            var lastSeen = Constants.DefaultUtcDate;
            var count = 0;
            var manual = false;
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
                manual = cp.IsManual;
            }
            if (count == 0)
                return new AggCheckpoint(default, 
                    default, default, 
                    0, false);
            return new AggCheckpoint(riderId, timestamp, lastSeen, count, manual);
        }

        public AggCheckpoint Add(Checkpoint cp)
        {
            if (RiderId != cp.RiderId)
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            var record = new []{new KeyValuePair<string, int>(cp.GetType().Name, 1)};
            
            return new AggCheckpoint(RiderId, 
                Timestamp.TakeSmaller(cp.Timestamp),
                LastSeen.TakeLarger(cp.Timestamp),
                Count + 1, IsManual || cp.IsManual);
        }
    }
}