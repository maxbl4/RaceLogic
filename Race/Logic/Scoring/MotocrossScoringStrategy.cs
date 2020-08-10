using System.Collections.Generic;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.Scoring
{
    public static class WellKnownScoringStrategies
    {
        public static RoundScoringStrategy Motocross()
        {
            return RoundScoringStrategy.FromFirstPlacePoints(20, 1, new []{5, 3, 2, 1});
        }
        
        public static RoundScoringStrategy XsrCountryCross(IEnumerable<RoundPosition> positions)
        {
            return RoundScoringStrategy.FromFinishers(positions, 1, new []{5, 3, 2, 1});
        }
        
        public static RoundScoringStrategy BraaapCountryCross(IEnumerable<RoundPosition> positions)
        {
            return RoundScoringStrategy.FromFinishers(positions);
        }
    }
}