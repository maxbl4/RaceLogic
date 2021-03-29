using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class SessionDto : IHasId<SessionDto>, IHasName, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Guid EventId { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public Guid FinishCriteriaId { get; set; }
        public List<Guid> ClassIds { get; set; } = new();
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}