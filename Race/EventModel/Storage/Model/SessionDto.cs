using System;
using System.Collections.Generic;
using maxbl4.Race.EventModel.Storage.Identifier;
using maxbl4.Race.EventModel.Storage.Traits;

namespace maxbl4.Race.EventModel.Storage.Model
{
    public class SessionDto : IHasId<SessionDto>, IHasName, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<SessionDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        public bool Published { get; set; }

        public Id<EventDto> EventId {get;set;}
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public Id<FinishCriteriaDto> FinishCriteriaId { get; set; }
        public List<Id<ClassDto>> ClassIds { get; set; } = new List<Id<ClassDto>>();
    }
}