using System;
using maxbl4.Race.EventModel.Storage.Identifier;
using maxbl4.Race.EventModel.Storage.Traits;

namespace maxbl4.Race.EventModel.Storage.Model
{
    public class RecordingSessionDto: IHasId<RecordingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<RecordingSessionDto> Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public Id<SessionDto> SessionId { get; set; }
    }

    public class RatingSessionDto : IHasId<RatingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<RatingSessionDto> Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public Id<FinishCriteriaDto> FinishCriteriaId { get; set; }
    }
}