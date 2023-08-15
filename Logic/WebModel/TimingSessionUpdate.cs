using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.WebModel
{
    public class TimingSessionUpdate: IHasId<TimingSessionUpdate>, IHasTimestamp
    {
        public Id<TimingSessionUpdate> Id { get; set; }
        public Id<TimingSessionDto> TimingSessionId { get; set; }
        public List<RoundPosition> Rating { get; set; }
        public List<Checkpoint> ResolvedCheckpoints { get; set; }
        public List<Rider> Riders { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
        public int MaxLapCount { get; set; }
    }

    public class RoundPosition
    {
        public int LapCount { get; set; }
        public List<Lap> Laps { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Finished { get; set; }
        public bool Started => LapCount > 0;
        public string RiderId { get; set; }
        public Rider Rider { get; set; }
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
        public string Number { get; set; }
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