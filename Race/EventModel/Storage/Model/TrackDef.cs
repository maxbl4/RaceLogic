using System;
using maxbl4.Race.EventModel.Storage.Identifier;
using maxbl4.Race.EventModel.Storage.Traits;

namespace maxbl4.Race.EventModel.Storage.Model
{
    public class TrackDef : IHasId<TrackDef>, IHasName, IHasTimestamp, IHasPublished, IHasSeed
    {
        public Id<TrackDef> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        
        public string MapLink { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
    }
}