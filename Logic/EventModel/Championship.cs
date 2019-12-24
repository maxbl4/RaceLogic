using System;
using System.Collections.Generic;

namespace maxbl4.Race.Logic.EventModel
{
    public class Championship : ITimestamp
    {
        public Guid ChampionshipId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? SeriesId { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public Series Series { get; set; }
        public List<Event> Events { get; set; }
        public List<Class> Classes { get; set; }
        public List<ChampionshipRiderResult> Results { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}