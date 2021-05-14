using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Storage.Model
{
    public class CheckpointServiceDto: IHasId<CheckpointServiceDto>, IHasName, IHasTimestamp
    {
        public Id<CheckpointServiceDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string RfidConnectionString { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}