using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using maxbl4.Infrastructure.Extensions.DictionaryExt;

namespace maxbl4.Race.Logic.Checkpoints
{
    public interface ITimestampAggregator<T> : IObserver<T>, IObservable<T>
        where T: class
    {
        IObservable<T> AggregatedCheckpoints { get; }
    }

    public class TimestampAggregator<T> : ITimestampAggregator<T>
        where T: class
    {
        private readonly TimeSpan window;
        private readonly Func<T, DateTime> timestampGetter;
        private readonly Func<T, string> keyGetter;
        private readonly Func<T, T, T> aggregator;
        readonly Subject<T> aggregatedCheckpoints = new Subject<T>();
        public IObservable<T> AggregatedCheckpoints => aggregatedCheckpoints;

        readonly Subject<T> checkpoints = new Subject<T>();
        public IObservable<T> Checkpoints => checkpoints;

        readonly Dictionary<string, T> aggregationCache = new Dictionary<string, T>();

        public TimestampAggregator(TimeSpan window, Func<T, DateTime> timestampGetter, Func<T, string> keyGetter, Func<T, T, T> aggregator)
        {
            this.window = window;
            this.timestampGetter = timestampGetter;
            this.keyGetter = keyGetter;
            this.aggregator = aggregator;
        }
        
        public void OnCompleted()
        {
            checkpoints.OnCompleted();
            foreach (var agg in aggregationCache.Values.OrderBy(timestampGetter))
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

        public void OnNext(T cp)
        {
            foreach (var c in ApplyWindow(cp, window, aggregationCache))
            {
                if (c.aggregated)
                    aggregatedCheckpoints.OnNext(c.item);
                else
                    checkpoints.OnNext(c.item);
            }
        }
        
        /// <summary>
        /// Will apply sliding time window to checkpoints list
        /// and aggregate by taking the first occurrence.
        /// Assumes, that input has full information, will the sort the list before aggregation
        /// </summary>
        /// <param name="items">All checkpoints should have Timestamp set</param>
        /// <param name="window"></param>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<T> AggregateOnce(List<T> items, TimeSpan window, IComparer<T> comparer)
        {
            items.Sort(comparer);
            var result = new List<T>();
            var aggregationCache = new Dictionary<string, T>();
            foreach (var cp in items)
            {
                result.AddRange(ApplyWindow(cp, window, aggregationCache).Where(x => x.aggregated).Select(x => x.item));
            }
            result.AddRange(aggregationCache.Values);
            result.Sort(comparer);
            return result;
        }

        IEnumerable<(T item, bool aggregated)> ApplyWindow(T itm, TimeSpan window, Dictionary<string, T> aggregationCache)
        {
            var key = keyGetter(itm);
            var agg = aggregationCache.Get(key);
            if (agg == null || timestampGetter(itm) - timestampGetter(agg) > window)
            {
                yield return (itm, false);
                if (agg != null)
                    yield return (agg, true);
                aggregationCache[key] = aggregator(itm, null);
            }
            else
            {
                aggregationCache[key] = aggregator(agg, itm);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return checkpoints.Subscribe(observer);
        }
    }

    public static class TimestampAggregatorConfigurations
    {
        public static TimestampAggregator<Checkpoint> ForCheckpoint(TimeSpan window)
        {
            return new TimestampAggregator<Checkpoint>(window, cp => cp.Timestamp, cp => cp.RiderId, (agg, cp) => cp == null ? agg.ToAggregated() : agg.AddToAggregated(cp));
        }
    }
}