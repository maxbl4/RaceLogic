using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class ChampionshipDto : IHasId<ChampionshipDto>, IHasName, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<SeriesDto> SeriesId { get; set; }
        public Id<ChampionshipDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}