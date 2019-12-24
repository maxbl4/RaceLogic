using System;

namespace maxbl4.Race.Logic.EventModel
{
    public class NumberGroup: ITimestamp
    {
        public Guid NumberGroupId { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}