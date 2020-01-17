using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class RecordingSession: IHasId<RecordingSession>, IHasName, IHasSeed, IHasTimestamp
    {
        public Id<RecordingSession> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
        public Id<SessionDto> SessionDtoId { get; set; }
    }
}