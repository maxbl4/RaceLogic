using System;
using maxbl4.Race.EventModel.Storage.Identifier;
using maxbl4.Race.EventModel.Storage.Traits;

namespace maxbl4.Race.EventModel.Storage.Model
{
    public class ChampionshipDto: IHasId<ChampionshipDto>, IHasName, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<ChampionshipDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        public bool Published { get; set; }
        
        public Id<SeriesDto> SeriesId { get; set; }
    }
}