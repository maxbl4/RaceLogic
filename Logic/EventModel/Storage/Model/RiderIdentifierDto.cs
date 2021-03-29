using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RiderIdentifierDto : IHasId<RiderIdentifierDto>, IHasTimestamp, IHasSeed
    {
        public Guid RiderProfileId { get; set; }
        public string Identifier { get; set; }
        public Guid Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}