using System.Collections.Generic;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSessionRatingUpdated
    {
        public List<RoundPosition> Rating { get; set; }
    }
}