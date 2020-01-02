using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace maxbl4.Race.Logic.EventModel
{
    public class Lap: ITimestamp
    {
        public Guid LapId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan AggDuration { get; set; }
        public int Number { get; set; }
        [Required]
        public Guid CheckpointId { get; set; }
        /// <summary>
        /// Closing checkpoint
        /// </summary>
        public Checkpoint Checkpoint { get; set; }
        [Required]
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration RiderRegistration{ get; set; }
        [Required]
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        [Required]
        public Guid RoundRiderResultId { get; set; }
        public RoundRiderResult RoundRiderResult { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        [NotMapped]
        public Guid RiderId
        {
            get => RiderRegistrationId;
            set => RiderRegistrationId = value;
        }

        public Lap PartialClone()
        {
            return new Lap {
                Start = Start,
                End = End,
                Duration = Duration,
                AggDuration = AggDuration,
                Number = Number,
                CheckpointId = CheckpointId,
                Checkpoint = Checkpoint,
                RiderRegistrationId = RiderRegistrationId,
                RiderRegistration = RiderRegistration,
                ScheduleId = ScheduleId,
                Schedule = Schedule
            };
        }
    }
}