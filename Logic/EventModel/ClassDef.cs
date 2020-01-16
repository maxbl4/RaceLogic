using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class ClassDef: IHasIdentifiers<ClassDef>, IHasTimestamp, IHasSeed, IHasPublished
    {
        public Id<ClassDef> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        
        public Guid ChampionshipId { get; set; }
        public bool HasAgeLimit { get; set; }
        public Guid? NumberGroupId { get; set; }
    }
}