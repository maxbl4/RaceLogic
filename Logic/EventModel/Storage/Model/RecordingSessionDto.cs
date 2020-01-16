using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RecordingSessionDto: IHasId<RecordingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<RecordingSessionDto> Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}