using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using maxbl4.Infrastructure.Extensions.DictionaryExt;

namespace maxbl4.Race.Logic.Checkpoints
{
    public interface ICheckpointAggregator : IObserver<Checkpoint>, IObservable<Checkpoint>
    {
        IObservable<Checkpoint> AggregatedCheckpoints { get; }
    }

    public class TimestampCheckpointAggregator : ICheckpointAggregator
    {
        private readonly TimeSpan window;
        readonly Subject<Checkpoint> aggregatedCheckpoints = new Subject<Checkpoint>();
        public IObservable<Checkpoint> AggregatedCheckpoints => aggregatedCheckpoints;

        readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();
        public IObservable<Checkpoint> Checkpoints => checkpoints;

        readonly Dictionary<string, Checkpoint> aggregationCache = new Dictionary<string, Checkpoint>();

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

        public void OnNext(Checkpoint cp)
        {
            foreach (var c in ApplyWindow(cp, window, aggregationCache))
            {
                if (c.Aggregated)
                    aggregatedCheckpoints.OnNext(c);
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
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public static List<Checkpoint> AggregateOnce(List<Checkpoint> checkpoints, TimeSpan window)
        {
            checkpoints.Sort(Checkpoint.TimestampComparer);
            var result = new List<Checkpoint>();
            var aggregationCache = new Dictionary<string, Checkpoint>();
            foreach (var cp in checkpoints)
            {
                result.AddRange(ApplyWindow(cp, window, aggregationCache).Where(x => x.Aggregated));
            }
            result.AddRange(aggregationCache.Values);
            result.Sort(Checkpoint.TimestampComparer);
            return result;
        }

        static IEnumerable<Checkpoint> ApplyWindow(Checkpoint cp, TimeSpan window, Dictionary<string, Checkpoint> aggregationCache)
        {
            var agg = aggregationCache.Get(cp.RiderId);
            if (agg == null || cp.Timestamp - agg.Timestamp > window)
            {
                yield return cp;
                if (agg != null)
                    yield return agg;
                aggregationCache[cp.RiderId] = cp.ToAggregated();
            }
            else
            {
                agg.AddToAggregated(cp);
            }
        }

        public IDisposable Subscribe(IObserver<Checkpoint> observer)
        {
            return checkpoints.Subscribe(observer);
        }
    }
}