using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    // Result for rider in class in single race 
    public class ClassRiderResult: IHasTimestamp
    {
        public Guid ClassRiderResultId { get; set; }
        public Guid ClassId { get; set; }
        public ClassDef Class { get; set; }
        public Guid EventId { get; set; }
        public Event Event { get; set; }
        [Required]
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration RiderRegistration { get; set; }
        public List<RoundRating> RoundRatings { get; set; }
        public List<RoundRiderResult> RoundRiderResults { get; set; }
        public int Position { get; set; }
        public bool Finished { get; set; }
        public bool Started { get; set; }
        public bool Dsq { get; set; }
        public int Points { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
        public Guid RiderId => RiderRegistrationId;
    }
}