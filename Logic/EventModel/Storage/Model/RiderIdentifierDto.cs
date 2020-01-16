using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RiderIdentifierDto : IHasId<RiderIdentifierDto>, IHasTimestamp, IHasSeed
    {
        public Id<RiderIdentifierDto> Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }

        public Id<RiderProfileDto> RiderProfileId { get; set; }
        public string Identifier { get; set; }
    }
}