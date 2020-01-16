using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class SeriesDef : IHasIdentifiers<SeriesDef>, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<SeriesDef> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        public bool Published { get; set; }
    }
}