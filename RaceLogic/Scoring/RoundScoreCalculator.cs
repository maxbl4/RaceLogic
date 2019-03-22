using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Model;

namespace RaceLogic.Scoring
{
    public class RoundScoreCalculator
    {
        public class ScoringStrategy
        {
            public ScoringMode Mode { get; }
            public enum ScoringMode
            {
                ByExample,
                StaticStart
            }
            
            //public int 

            /// <summary>
            /// [25,22,20,18,17,16....] - handwritten score examples with static score for first place
            /// </summary>
            public int[] ScoreExamples { get; }
        }

        public List<RoundScore<TRiderId>> Calculate<TRiderId>(IEnumerable<RoundPosition<TRiderId>> positions)
            where TRiderId : IEquatable<TRiderId>
        {
            /* Варианта порядка:
             *     1. Чисто по кругам и времени
             *     2. Приоритет финишеров (то есть вначало списка выходят, те кто финишировали)
             * Начисление очков:
             *     1. Начислять финишировавшим и стартовавшим
             *     2. Только финишировавшим
             * Расчёт количества очков (примеры):
             *     1. От числа финишировавших вниз по одному очку
             *     2. Фиксированное количество зачётных мест. Например первые 20
             *     3. Бонус за первые места, например в мотокроссе первые 4 места имеют бонусы 5,3,2,1 сверх очков из №2
             * 
             */
            
            var allPositions = positions.ToList();
            return allPositions
                .Select((x, i) => new RoundScore<TRiderId>(x, i + 1, allPositions.Count - i))
                .ToList();
        }
    }
}