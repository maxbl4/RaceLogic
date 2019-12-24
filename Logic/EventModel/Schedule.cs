using System;
using System.Collections.Generic;

namespace maxbl4.Race.Logic.EventModel
{
    public class Schedule: ITimestamp
    {
        public Guid ScheduleId { get; set; }
        public string Name {get;set;}
        public string Description {get;set;}
        public DateTimeOffset StartTime {get;set;}
        public TimeSpan Duration {get;set;}
        public TimeSpan? MinLap { get; set; }
        public Guid EventId {get;set;}
        public Event Event { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public List<RoundRating> RoundRatings { get; set; }
        public List<ScheduleToClass> ClassLinks { get; set; }
        public List<Checkpoint> Checkpoints { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTimeOffset ActualStartTime { get; set; }
    }
}