using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;
using RaceLogic.Model;

namespace RaceLogic
{
    //TODO: Рефакторить PositionAggregator, чтобы он использовал обычный Position
    public class PositionAggregator
    {
        public List<AggPosition<TRiderId, TInput>> Aggregate<TRiderId, TInput>(List<List<TInput>> rounds)
            where TRiderId: IEquatable<TRiderId>
            where TInput: IPosition<TRiderId>
        {
            var rating = new Dictionary<TRiderId,AggPosition<TRiderId, TInput>>();
            for (var i = 0; i < rounds.Count; i++)
            {
                var round = rounds[i];
                foreach (var position in round)
                {
                    rating.UpdateOrAdd(position.RiderId,
                        x => x.AddPosition(position, i), new AggPosition<TRiderId, TInput>(position.RiderId));
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