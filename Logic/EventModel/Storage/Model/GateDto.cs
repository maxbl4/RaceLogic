using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Storage.Model
{
    public class GateDto: IHasId<GateDto>, IHasName, IHasTimestamp
    {
        public Id<GateDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CheckpointServiceAddress { get; set; }
        public string RfidConnectionString { get; set; }
        public bool RfidSupported { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}