using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Storage.Model
{
    public class DropCheckpointDto : IHasId<DropCheckpointDto>, IHasTimestamp, IHasSeed
    {
        public Guid TargetId { get; set; }
        public Guid Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}