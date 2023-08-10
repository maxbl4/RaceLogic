using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.WebModel
{
    public class TimingSessionUpdate: IHasId<TimingSessionUpdate>
    {
        public Id<TimingSessionUpdate> Id { get; set; }
        public Id<TimingSessionDto> TimingSessionId { get; set; }
        public List<RoundPosition> Rating { get; set; }

        public static TimingSessionUpdate From(Id<TimingSessionDto> id, 
            List<RoundTiming.RoundPosition> rating, IAutoMapperProvider autoMapper)
        {
            return new TimingSessionUpdate
            {
                Id = id.Value,
                TimingSessionId = id,
                Rating = autoMapper.Map<List<RoundPosition>>(rating)
            };
        }
    }

    public class RoundPosition
    {
        public int LapCount { get; private set; }
        public List<Lap> Laps { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public bool Finished { get; private set; }
        public bool Started => LapCount > 0;
        public string RiderId { get; private set; }
        //public Rider Rider { get; private set; }
    }
    
    public class Lap
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan AggDuration { get; set;}
        public int SequentialNumber { get; set; }
    }

    public class Rider: IHasPersonName
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string ParentName { get; set; }
        public string LastName { get; set; }
        public Class Class { get; set; }
        public bool IsWrongSession { get; set; }
    }

    public class Class
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}