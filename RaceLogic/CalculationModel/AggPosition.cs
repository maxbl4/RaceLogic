using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;

namespace RaceLogic.CalculationModel
{
    public class AggPosition<TKey,TPosition> : IPosition<TKey>, IComparable<AggPosition<TKey,TPosition>>, IComparable
        where TKey: struct, IComparable, IComparable<TKey>, IEquatable<TKey>
        where TPosition: IPosition<TKey>
    {
        public int Points { get; set; }
        public int AggPoints { get; set; }
        public int Position { get; set; }
        public bool Dsq { get; set; }
        public TKey RiderId { get; }
        public int MaxRoundIndex { get; private set; }
        public int PositionInLastRound { get; private set; }
        public int PointsInLastRound { get; private set; }
        public Dictionary<int, int> PositionHistogram { get; } = new Dictionary<int, int>();
        public List<TPosition> OriginalPositions { get; set; } = new List<TPosition>();

        public AggPosition(TKey riderId)
        {
            RiderId = riderId;
        }

        public AggPosition<TKey,TPosition> AddPosition(TPosition position, int roundIndex)
        {
            if (!RiderId.Equals(position.RiderId))
                throw new ArgumentException($"RiderId should be same as initial ({RiderId}), but was ({position.RiderId})", nameof(position));
            Points += position.Points;
            if (position.Points > 0) // Ignore positions with 0 points
            {
                OriginalPositions.Add(position);
                PositionHistogram.UpdateOrAdd(position.Position, v => v + 1);
            }

            if (MaxRoundIndex <= roundIndex)
            {
                MaxRoundIndex = roundIndex;
                PointsInLastRound = position.Points;
                PositionInLastRound = position.Position;
            }
            return this;
        }

        private List<PositionHistogramItem> orderedHistogramItems;
        List<PositionHistogramItem> GetOrderedHistogramItems(bool cached = true)
        {
            if (cached && orderedHistogramItems != null) return orderedHistogramItems;
            return orderedHistogramItems = PositionHistogram
                .Select(x => new PositionHistogramItem {Position = x.Key, Count = x.Value})
                .OrderBy(x => x.Position).ToList();
        }

        public int CompareTo(AggPosition<TKey,TPosition> other)
        {
            // 1. Compare points
            // 2. Compare number of rounds
            // 3. Compare best place, not points!
            // 4. Compare places in last round
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            if ((new Guid("08d5cb28-d94f-75b3-b99d-f267fa74aae0").Equals(RiderId) &&
                new Guid("08d5d14a-91c0-2abd-fb7f-bf760039ee03").Equals(other.RiderId)
                ||new Guid("08d5cb28-d94f-75b3-b99d-f267fa74aae0").Equals(other.RiderId) &&
                new Guid("08d5d14a-91c0-2abd-fb7f-bf760039ee03").Equals(RiderId))
                && typeof(TPosition).Name == "ClassRiderResult")
            {
            }

            var points = other.Points.CompareTo(Points);
            if (points != 0) return points;
            var roundsCount = other.OriginalPositions.Count.CompareTo(OriginalPositions.Count);
            if (roundsCount != 0) return roundsCount;
            var pointsHistogram = ComparePointsHistogram(other);
            if (pointsHistogram != 0) return pointsHistogram;
            return CompareLastRound(other);
        }

        public int ComparePointsHistogram(AggPosition<TKey,TPosition> other)
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

        public int CompareLastRound(AggPosition<TKey,TPosition> other)
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
            return CompareTo(obj as AggPosition<TKey,TPosition>);
        }
    }

    public class PositionHistogramItem
    {
        public int Position { get; set; }
        public int Count { get; set; }
    }
}