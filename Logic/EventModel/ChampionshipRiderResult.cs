using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class ChampionshipRiderResult : IHasTimestamp, IHasSeed
    {
        public Guid ChampionshipRiderResultId { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public bool Dsq { get; set; }
        public Guid ClassId { get; set; }
        public ClassDef Class { get; set; }
        public Guid ChampionshipId { get; set; }
        public Guid RiderRegistrationId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
    }
}