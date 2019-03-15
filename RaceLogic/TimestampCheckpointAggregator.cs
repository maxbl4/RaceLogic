using System;
using System.Collections.Generic;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;
using RaceLogic.Model;

namespace RaceLogic
{
    public class TimestampCheckpointAggregator
    {
        /// <summary>
        /// Will apply sliding time window to checkpoints list
        /// and aggregate by taking the first occurrence.
        /// Assumes, that input has full information, will the sort the list before aggregation
        /// </summary>
        /// <param name="checkpoints">All checkpoints should have Timestamp set</param>
        /// <param name="window"></param>
        /// <typeparam name="TRiderId"></typeparam>
        /// <returns></returns>
        public static List<AggCheckpoint<TRiderId>> AggregateOnce<TRiderId>(List<Checkpoint<TRiderId>> checkpoints, TimeSpan window)
            where TRiderId : IEquatable<TRiderId>
        {
            checkpoints.Sort(Checkpoint<TRiderId>.TimestampComparer);
            var result = new List<AggCheckpoint<TRiderId>>();
            var aggRecords = new Dictionary<TRiderId, AggCheckpoint<TRiderId>>();
            foreach (var cp in checkpoints)
            {
                if (!cp.HasTimestamp)
                {
                    throw new ArgumentException($"All checkpoints must have Timestamp set", nameof(checkpoints));
                }
                var agg = aggRecords.Get(cp.RiderId);
                if (agg == null || cp.Timestamp - agg.Timestamp > window)
                {
                    if (agg != null)
                        result.Add(agg);
                    aggRecords[cp.RiderId] = AggCheckpoint<TRiderId>.From(cp);
                }
                else
                {
                    aggRecords[cp.RiderId] = agg.Add(cp);
                }
            }
            result.AddRange(aggRecords.Values);
            result.Sort(Checkpoint<TRiderId>.TimestampComparer);
            return result;
        }
    }
}