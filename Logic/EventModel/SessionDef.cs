using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class SessionDef : IHasIdentifiers<SessionDef>, IHasTimestamp, IHasSeed
    {
        public Id<SessionDef> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public Guid FinishCriteriaId { get; set; }
        public List<Guid> ClassIds { get; set; }
    }
}