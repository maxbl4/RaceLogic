using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace maxbl4.Race.Logic.EventModel
{
    public class RoundRiderResult: ITimestamp
    {
        public Guid RoundRiderResultId { get; set; }
        [Required]
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration RiderRegistration { get; set; }
        public Guid RoundRatingId { get; set; }
        public Guid? ClassRiderResultId { get; set; }
        public RoundRating RoundRating { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public int LapsCount { get; set; }
        public List<Lap> Laps { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Finished { get; set; }
        public bool Started { get; set; }
        public bool Dsq { get; set; }
        
        [NotMapped]
        public Guid RiderId
        {
            get => RiderRegistrationId;
            set => RiderRegistrationId = value;
        }

        public RoundRiderResult PartialClone()
        {
            return new RoundRiderResult{
                RiderRegistrationId = RiderRegistrationId,
                RiderRegistration = RiderRegistration,
                Position = Position,
                Points = Points,
                LapsCount = LapsCount,
                Laps = Laps?.Select(x => x.PartialClone()).ToList(),
                Duration = Duration,
                Start = Start,
                End = End,
                Started = Started,
                Finished = Finished
            };
        }
    }
}