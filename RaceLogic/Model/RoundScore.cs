using System;

namespace RaceLogic.Model
{
    public class RoundScore<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        public TRiderId RiderId => PositionDetails.RiderId;
        public int Points { get; }
        public int Position { get; }
        public RoundPosition<TRiderId> PositionDetails { get; }

        public RoundScore(RoundPosition<TRiderId> positionDetails, int position, int points)
        {
            
            PositionDetails = positionDetails;
            Position = position;
            Points = points;
        }
    }
}