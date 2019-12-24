using System;
using System.Collections.Generic;

namespace maxbl4.Race.Logic.EventModel
{
    public class Series : ITimestamp
    {
        public Guid SeriesId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public List<Championship> Championships { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}