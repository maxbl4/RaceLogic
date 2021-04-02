using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class SessionDto : IHasId<SessionDto>, IHasName, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<EventDto> EventId { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public DateTime StartTime { get; set; }
        public FinishCriteriaDto FinishCriteria { get; set; }
        public List<Id<ClassDto>> ClassIds { get; set; } = new();
        public Id<SessionDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}