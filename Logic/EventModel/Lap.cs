using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class Lap: IHasTimestamp
    {
        public Guid LapId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan AggDuration { get; set; }
        public int Number { get; set; }
        public Guid CheckpointId { get; set; }
        public Guid RiderRegistrationId { get; set; }
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public Guid RoundRiderResultId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}