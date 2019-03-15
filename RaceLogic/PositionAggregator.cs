using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;
using RaceLogic.Model;

namespace RaceLogic
{
    public class PositionAggregator
    {
        public List<AggPosition<TKey, TInput>> Aggregate<TKey, TInput>(List<List<TInput>> rounds)
            where TKey: struct, IComparable, IComparable<TKey>, IEquatable<TKey>
            where TInput: IPosition<TKey>
        {
            var rating = new Dictionary<TKey,AggPosition<TKey, TInput>>();
            for (var i = 0; i < rounds.Count; i++)
            {
                var round = rounds[i];
                foreach (var position in round)
                {
                    rating.UpdateOrAdd(position.RiderId,
                        x => x.AddPosition(position, i), new AggPosition<TKey, TInput>(position.RiderId));
                }
            }
            var maxPoints = rating.Values.Count(x => x.Points > 0);
            var result = rating.Values.OrderBy(x => x)
                .Select((x, i) =>
                {
                    x.Position = i + 1;
                    x.AggPoints = x.Points;
                    x.Points = Math.Max(0, maxPoints - i);
                    return x; })
                .ToList();
            
            return result;
        }
    }
}