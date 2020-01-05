using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cli.BraaapWebModel
{
    // Result for rider in class in championship
    public class ChampionshipRiderResult : ITimestamp
    {
        public Guid ChampionshipRiderResultId { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public bool Dsq { get; set; }
        public Guid ClassId { get; set; }
        public Class? Class { get; set; }
        public Guid ChampionshipId { get; set; }
        public Championship? Championship { get; set; }
        public List<ClassRiderResult>? ClassRiderResults { get; set; }
        [Required]
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration? RiderRegistration { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
        public Guid RiderId => RiderRegistrationId;
    }
}