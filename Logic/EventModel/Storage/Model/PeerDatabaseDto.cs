using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class PeerDatabaseDto : IHasId<PeerDatabaseDto>, IHasName, IHasTimestamp
    {
        public Id<PeerDatabaseDto> Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string BaseUri { get; set; }
        public DateTime LastSyncTimestamp { get; set; } = Constants.DefaultUtcDate;
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}