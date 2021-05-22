using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Storage.Model
{
    public class SeedDataDto : IHasId<SeedDataDto>, IHasTimestamp
    {
        public Id<SeedDataDto> Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}