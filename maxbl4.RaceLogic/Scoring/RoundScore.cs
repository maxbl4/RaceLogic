using maxbl4.RaceLogic.RoundTiming;

namespace maxbl4.RaceLogic.Scoring
{
    public class RoundScore
    {
        private readonly string riderId;
        public string RiderId => PositionDetails != null ? PositionDetails.RiderId : riderId;
        public int Points { get; }
        public int Position { get; }
        public RoundPosition PositionDetails { get; }

        public RoundScore(RoundPosition positionDetails, int position, int points)
        {
            PositionDetails = positionDetails;
            Position = position;
            Points = points;
        }
        
        public RoundScore(string riderId, int position, int points)
        {
            this.riderId = riderId;
            Position = position;
            Points = points;
        }
    }
}