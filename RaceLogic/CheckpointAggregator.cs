using System;
using System.Collections.Generic;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;

namespace RaceLogic
{
    public class CheckpointAggregator
    {
        /// <summary>
        /// Will apply a sliding window to checkpoints
        /// returning only first occurence within a window
        /// </summary>
        /// <param name="rawRecords"></param>
        /// <param name="minLap"></param>
        /// <typeparam name="TRiderId"></typeparam>
        /// <typeparam name="TCheckpoint"></typeparam>
        /// <returns></returns>
        public List<TCheckpoint> AggregateByMinLap<TRiderId, TCheckpoint>(IEnumerable<TCheckpoint> rawRecords, TimeSpan minLap)
            where TRiderId : IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
            where TCheckpoint: class, IAggCheckpoint<TRiderId>
        {
            var result = new List<TCheckpoint>();
            var aggRecords = new Dictionary<TRiderId, TCheckpoint>();
            foreach (var record in rawRecords)
            {
                var agg = aggRecords.Get(record.RiderId);
                if (agg == null || record.Timestamp - agg.Timestamp > minLap)
                {
                    if (agg != null)
                        result.Add(agg);
                    aggRecords[record.RiderId] = record;
                }
                else
                {
                    agg.TotalCount++;
                    agg.RfidCount += record.RfidCount;
                    agg.ManualCount += record.ManualCount;
                }
            }

            result.AddRange(aggRecords.Values);
            result.Sort(TimestampRelationalComparer<TRiderId, TCheckpoint>.Instance);
            return result;
        }
        
        private sealed class TimestampRelationalComparer<TRiderId, TCheckpoint> : Comparer<TCheckpoint>
            where TRiderId : IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
            where TCheckpoint: ICheckpoint<TRiderId> 
        {
            public static readonly TimestampRelationalComparer<TRiderId, TCheckpoint> Instance = 
                new TimestampRelationalComparer<TRiderId, TCheckpoint>();
            public override int Compare(TCheckpoint x, TCheckpoint y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }
    }
}