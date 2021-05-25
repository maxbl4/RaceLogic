using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.WebModel
{
    public class TimingSessionUpdate
    {
        public List<RoundPosition> Rating { get; set; }
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
        public Rider Rider { get; private set; }
    }
    
    public class Lap
    {
        public DateTime Start { get; }
        public DateTime End { get; }
        public TimeSpan Duration { get; }
        public TimeSpan AggDuration { get; }
        public int SequentialNumber { get; }
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