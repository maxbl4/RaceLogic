using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class ClassDto : IHasId<ClassDto>, IHasName, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<ChampionshipDto> ChampionshipId { get; set; }
        public Id<NumberGroupDto> NumberGroupId { get; set; }
        public Id<ClassDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}