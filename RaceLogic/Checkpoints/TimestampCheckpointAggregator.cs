using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using RaceLogic.Extensions;

namespace RaceLogic.Checkpoints
{
    public interface ICheckpointAggregator<TRiderId> : IObserver<Checkpoint<TRiderId>>, IObservable<Checkpoint<TRiderId>>
        where TRiderId : IEquatable<TRiderId>
    {
        IObservable<AggCheckpoint<TRiderId>> AggregatedCheckpoints { get; }
    }

    public class TimestampCheckpointAggregator<TRiderId> : ICheckpointAggregator<TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        private readonly TimeSpan window;
        readonly Subject<AggCheckpoint<TRiderId>> aggregatedCheckpoints = new Subject<AggCheckpoint<TRiderId>>();
        public IObservable<AggCheckpoint<TRiderId>> AggregatedCheckpoints => aggregatedCheckpoints;

        readonly Subject<Checkpoint<TRiderId>> checkpoints = new Subject<Checkpoint<TRiderId>>();
        public IObservable<Checkpoint<TRiderId>> Checkpoints => checkpoints;

        readonly Dictionary<TRiderId, AggCheckpoint<TRiderId>> aggregationCache = new Dictionary<TRiderId, AggCheckpoint<TRiderId>>();

        public TimestampCheckpointAggregator(TimeSpan window)
        {
            this.window = window;
        }
        
        public void OnCompleted()
        {
            checkpoints.OnCompleted();
            foreach (var agg in aggregationCache.Values.OrderBy(x => x.Timestamp))
            {
                aggregatedCheckpoints.OnNext(agg);
            }
            aggregatedCheckpoints.OnCompleted();
        }

        public void OnError(Exception error)
        {
            checkpoints.OnError(error);
            aggregatedCheckpoints.OnError(error);
        }

        public void OnNext(Checkpoint<TRiderId> cp)
        {
            foreach (var c in ApplyWindow(cp, window, aggregationCache))
            {
                if (c is AggCheckpoint<TRiderId> agg)
                    aggregatedCheckpoints.OnNext(agg);
                else
                    checkpoints.OnNext(c);
            }
        }
        
        /// <summary>
        /// Will apply sliding time window to checkpoints list
        /// and aggregate by taking the first occurrence.
        /// Assumes, that input has full information, will the sort the list before aggregation
        /// </summary>
        /// <param name="checkpoints">All checkpoints should have Timestamp set</param>
        /// <param name="window"></param>
        /// <typeparam name="TRiderId"></typeparam>
        /// <returns></returns>
        public static List<AggCheckpoint<TRiderId>> AggregateOnce(List<Checkpoint<TRiderId>> checkpoints, TimeSpan window)
        {
            checkpoints.Sort(Checkpoint<TRiderId>.TimestampComparer);
            var result = new List<AggCheckpoint<TRiderId>>();
            var aggregationCache = new Dictionary<TRiderId, AggCheckpoint<TRiderId>>();
            foreach (var cp in checkpoints)
            {
                result.AddRange(ApplyWindow(cp, window, aggregationCache).OfType<AggCheckpoint<TRiderId>>());
            }
            result.AddRange(aggregationCache.Values);
            result.Sort(Checkpoint<TRiderId>.TimestampComparer);
            return result;
        }

        static IEnumerable<Checkpoint<TRiderId>> ApplyWindow(Checkpoint<TRiderId> cp, TimeSpan window, Dictionary<TRiderId, AggCheckpoint<TRiderId>> aggregationCache)
        {
            if (!cp.HasTimestamp)
                throw new ArgumentException($"All checkpoints must have Timestamp set", nameof(checkpoints));
            
            var agg = aggregationCache.Get(cp.RiderId);
            if (agg == null || cp.Timestamp - agg.Timestamp > window)
            {
                yield return cp;
                if (agg != null)
                    yield return agg;
                aggregationCache[cp.RiderId] = AggCheckpoint<TRiderId>.From(cp);
            }
            else
            {
                aggregationCache[cp.RiderId] = agg.Add(cp);
            }
        }

        public IDisposable Subscribe(IObserver<Checkpoint<TRiderId>> observer)
        {
            return checkpoints.Subscribe(observer);
        }
    }
}