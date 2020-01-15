using System;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class ScheduleToClass: IHasTimestamp
    {
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public Guid ClassId { get; set; }
        public ClassDef Class { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}