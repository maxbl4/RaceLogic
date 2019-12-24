using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace maxbl4.Race.Logic.EventModel
{
    public class RoundRating: ITimestamp
    {
        public Guid RoundRatingId { get; set; }
        [Required]
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public Guid? ClassId { get; set; }
        public Class Class { get; set; }
        public List<RoundRiderResult> Results { get; set; }

        public Guid? ClassRiderResultId { get; set; }
        public ClassRiderResult ClassRiderResult { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}