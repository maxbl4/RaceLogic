using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class CheckpointDto: IHasId<CheckpointDto>, IHasSeed
    {
        public Id<CheckpointDto> Id { get; set; }
        public bool IsSeed { get; set; }
        
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public string RiderId { get; set; }
    }
}