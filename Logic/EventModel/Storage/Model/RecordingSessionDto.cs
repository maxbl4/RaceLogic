using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RecordingSessionDto : IHasId<RecordingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Guid SessionId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    public class RatingSessionDto : IHasId<RatingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Guid RecordingSessionId { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public Guid FinishCriteriaId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}