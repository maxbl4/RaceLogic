using System;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class FinishCriteriaDef: IHasIdentifiers, IHasTimestamp, IHasSeed
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        
        public TimeSpan Duration { get; set; }
        public int? TotalLaps { get; set; }
        public int LapsAfterDuration { get; set; }
        public bool SkipStartingCheckpoint { get; set; }
        public bool ForceFinishOnly { get; set; }
        public bool IndividualTiming { get; set; }
    }
}