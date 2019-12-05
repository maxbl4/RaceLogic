using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Extensions;

namespace maxbl4.Race.Logic.Scoring
{
    public class AggRoundScore : RoundScore, IComparable<AggRoundScore>, IComparable
    {
        public int AggPoints { get; }
        public int MaxRoundIndex { get; }
        public int PositionInLastRound { get; }
        public int PointsInLastRound { get; }
        public ReadOnlyDictionary<int, int> PositionHistogram { get; }
        public ReadOnlyCollection<RoundScore> OriginalScores { get; }
        
        private AggRoundScore(RoundScore score) 
            : base(score.RiderId, 0, score.Points) { }

        public AggRoundScore(string riderId) 
            : base(riderId, 0, 0) 
        { 
            OriginalScores = new ReadOnlyCollection<RoundScore>(new RoundScore[0]);
            PositionHistogram = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>());
        }
        
        public AggRoundScore(AggRoundScore baseScore, RoundScore score, int roundIndex) 
            : base(baseScore.RiderId, 0, baseScore.Points + score.Points) 
        { 
            if (baseScore.RiderId != score.RiderId)
                throw new ArgumentException($"RiderId should be same as initial ({RiderId}), but was ({score.RiderId})", nameof(score));
            if (score.Points > 0) // Ignore positions with 0 points
            {
                var scores = baseScore.OriginalScores.ToList();
                scores.Add(score);
                OriginalScores = new ReadOnlyCollection<RoundScore>(scores);
                var histogram = baseScore.PositionHistogram.ToDictionary(x => x.Key, x => x.Value);
                histogram.UpdateOrAdd(score.Position, v => v + 1);
                PositionHistogram = new ReadOnlyDictionary<int, int>(histogram);
            }
            else
            {
                OriginalScores = baseScore.OriginalScores;
                PositionHistogram = baseScore.PositionHistogram;
            }
            if (MaxRoundIndex <= roundIndex)
            {
                MaxRoundIndex = roundIndex;
                PointsInLastRound = score.Points;
                PositionInLastRound = score.Position;
            }
        }

        public AggRoundScore(AggRoundScore baseScore, int position, int points, int aggPoints)
            : base(baseScore.RiderId, position, points)
        {
            OriginalScores = baseScore.OriginalScores;
            PositionHistogram = baseScore.PositionHistogram;
            MaxRoundIndex = baseScore.MaxRoundIndex;
            PointsInLastRound = baseScore.PointsInLastRound;
            PositionInLastRound = baseScore.PositionInLastRound;
            AggPoints = aggPoints;
        }

        public AggRoundScore AddScore(RoundScore score, int roundIndex)
        {
            return new AggRoundScore(this, score, roundIndex);
        }

        private List<(int Position, int Count)> orderedHistogramItems;
        List<(int Position, int Count)> GetOrderedHistogramItems(bool cached = true)
        {
            if (cached && orderedHistogramItems != null) return orderedHistogramItems;
            return orderedHistogramItems = PositionHistogram
                .Select(x => (Position: x.Key, Count: x.Value))
                .OrderBy(x => x.Position).ToList();
        }

        public int CompareTo(AggRoundScore other)
        {
            // 1. Compare points
            // 2. Compare number of rounds
            // 3. Compare best place, not points!
            // 4. Compare places in last round
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var points = other.Points.CompareTo(Points);
            if (points != 0) return points;
            var roundsCount = other.OriginalScores.Count.CompareTo(OriginalScores.Count);
            if (roundsCount != 0) return roundsCount;
            var pointsHistogram = ComparePointsHistogram(other);
            if (pointsHistogram != 0) return pointsHistogram;
            return CompareLastRound(other);
        }

        public int ComparePointsHistogram(AggRoundScore other)
        {
            var others = other.GetOrderedHistogramItems();
            var currents = GetOrderedHistogramItems();
            for (var i = 0; i < Math.Max(currents.Count, others.Count); i++)
            {
                if (currents.Count <= i) return 1;
                if (others.Count <= i) return -1;
                // Compare place, not points. So lower is better
                var histogramPositions = currents[i].Position.CompareTo(others[i].Position);
                if (histogramPositions != 0) return histogramPositions;
                var histogramCount = others[i].Count.CompareTo(currents[i].Count);
                if (histogramCount != 0) return histogramCount;
            }

            return 0;
        }

        public int CompareLastRound(AggRoundScore other)
        {
            if (other.MaxRoundIndex < MaxRoundIndex) return -1;
            if (other.MaxRoundIndex > MaxRoundIndex) return 1;
            
            // Lower is better, but check for points
            if (PointsInLastRound > 0 && other.PointsInLastRound > 0)
                return PositionInLastRound.CompareTo(other.PositionInLastRound);
            // If one does not have points, the other will be better
            return other.PointsInLastRound.CompareTo(PointsInLastRound);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as AggRoundScore);
        }
    }
}