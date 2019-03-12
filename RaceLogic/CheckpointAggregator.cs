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
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TCheckpoint"></typeparam>
        /// <returns></returns>
        public List<TCheckpoint> AggregateByMinLap<TKey, TCheckpoint>(IEnumerable<TCheckpoint> rawRecords, TimeSpan minLap)
            where TKey : struct, IComparable, IComparable<TKey>, IEquatable<TKey>
            where TCheckpoint: class, IAggCheckpoint<TKey>
        {
            var result = new List<TCheckpoint>();
            var aggRecords = new Dictionary<TKey, TCheckpoint>();
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
            result.Sort(TimestampRelationalComparer<TKey, TCheckpoint>.Instance);
            return result;
        }
        
        private sealed class TimestampRelationalComparer<TKey, TCheckpoint> : Comparer<TCheckpoint>
            where TKey : struct, IComparable, IComparable<TKey>, IEquatable<TKey>
            where TCheckpoint: ICheckpoint<TKey> 
        {
            public static readonly TimestampRelationalComparer<TKey, TCheckpoint> Instance = 
                new TimestampRelationalComparer<TKey, TCheckpoint>();
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