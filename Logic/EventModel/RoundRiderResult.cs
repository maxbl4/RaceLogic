using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class RoundRiderResult: IHasTimestamp
    {
        public Guid RoundRiderResultId { get; set; }
        public Guid RiderRegistrationId { get; set; }
        public Guid RoundRatingId { get; set; }
        public Guid? ClassRiderResultId { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public int LapsCount { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Finished { get; set; }
        public bool Started { get; set; }
        public bool Dsq { get; set; }
    }
}