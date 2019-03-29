using System;
using System.Collections.Generic;
using RaceLogic.Model;

namespace RaceLogic.Scoring
{
    public static class WellKnownScoringStrategies<TRiderId>
        where TRiderId : IEquatable<TRiderId>
    {
        public static RoundScoringStrategy<TRiderId> Motocross()
        {
            return RoundScoringStrategy<TRiderId>.FromFirstPlacePoints(20, 1, new []{5, 3, 2, 1});
        }
        
        public static RoundScoringStrategy<TRiderId> XsrCountryCross(IEnumerable<RoundPosition<TRiderId>> positions)
        {
            return RoundScoringStrategy<TRiderId>.FromFinishers(positions, 1, new []{5, 3, 2, 1});
        }
        
        public static RoundScoringStrategy<TRiderId> BraaapCountryCross(IEnumerable<RoundPosition<TRiderId>> positions)
        {
            return RoundScoringStrategy<TRiderId>.FromFinishers(positions);
        }
    }
}