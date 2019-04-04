using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using RaceLogic.RoundTiming;

namespace RaceLogic.Scoring
{
    public class RoundScoringStrategy<TRiderId>
        where TRiderId : IEquatable<TRiderId>
    {
        public int FirstPlacePoints { get; }
        public int SubstractBy { get; }
        /// <summary>
        /// True - assign points to riders who have started, but did not finish.
        /// False - assign poinst only to finishers
        /// </summary>
        public bool RateDnfs { get; }

        /// <summary>
        /// [25,22,20,18,17,16....] - handwritten static score examples starting with first place
        /// Also used as bonus scores with dynamic scoring
        /// </summary>
        public ReadOnlyCollection<int> StaticScores { get; }

        private RoundScoringStrategy(IEnumerable<int> staticScores, 
            int firstPlacePoints, int substractBy, bool rateDnfs)
        {
            FirstPlacePoints = firstPlacePoints;
            SubstractBy = substractBy;
            RateDnfs = rateDnfs;
            StaticScores = new ReadOnlyCollection<int>(staticScores?.ToArray() ?? new int[0]);
        }

        public static RoundScoringStrategy<TRiderId> FromStaticScores(IEnumerable<int> staticScores, bool rateDnfs = false)
        {
            return new RoundScoringStrategy<TRiderId>(staticScores, 0, 0, rateDnfs);
        }
        
        public static RoundScoringStrategy<TRiderId> FromFinishers(IEnumerable<RoundPosition<TRiderId>> positions, 
            int substractBy = 1, IEnumerable<int> bonusScores = null)
        {
            return new RoundScoringStrategy<TRiderId>(bonusScores, positions.Count(x => x.Finished), substractBy, false);
        }
        
        public static RoundScoringStrategy<TRiderId> FromStarters(IEnumerable<RoundPosition<TRiderId>> positions, 
            int substractBy = 1, IEnumerable<int> bonusScores = null)
        {
            return new RoundScoringStrategy<TRiderId>(bonusScores, positions.Count(x => x.Started), substractBy, true);
        }
        
        public static RoundScoringStrategy<TRiderId> FromFirstPlacePoints(int firstPlacePoints, int substractBy = 1, IEnumerable<int> bonusScores = null, 
            bool rateDnfs = false)
        {
            return new RoundScoringStrategy<TRiderId>(bonusScores, firstPlacePoints, substractBy, rateDnfs);
        }


        public IEnumerable<RoundScore<TRiderId>> Calculate(IEnumerable<RoundPosition<TRiderId>> positions, IEnumerable<TRiderId> expectedRiders = null)
        {
            if (expectedRiders == null)
                expectedRiders = new TRiderId[0];
            var ridersWithCheckpoints = new HashSet<TRiderId>();
            var position = 1;
            foreach (var rp in positions)
            {
                ridersWithCheckpoints.Add(rp.RiderId);
                yield return new RoundScore<TRiderId>(rp, position, GetScoreForRoundPosition(position, rp));
                position++;
            }
            foreach (var rp in expectedRiders
                .Where(x => !ridersWithCheckpoints.Contains(x))
                .Select(x => RoundPosition<TRiderId>.FromStartTime(x)))
            {
                yield return new RoundScore<TRiderId>(rp, position, GetScoreForRoundPosition(position, rp));
                position++;
            }
        }

        public int GetScoreForRoundPosition(int position, RoundPosition<TRiderId> roundPosition)
        {
            if (!roundPosition.Started) return 0;
            if (!RateDnfs && !roundPosition.Finished) return 0;
            return GetScoreForPosition(position);
        }

        public int GetScoreForPosition(int position)
        {
            if (position < 1) return 0;
            var staticScore = StaticScores.Count >= position ? StaticScores[position - 1] : 0;
            var dynamicScore = FirstPlacePoints - SubstractBy * (position - 1);
            if (dynamicScore < 0) dynamicScore = 0;
            return staticScore + dynamicScore;
        }
    }
}