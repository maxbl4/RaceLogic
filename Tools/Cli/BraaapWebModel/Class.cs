using System;
using System.Collections.Generic;

namespace Cli.BraaapWebModel
{
    public class Class: ITimestamp
    {
        public Guid ClassId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid ChampionshipId { get; set; }
        public bool Published { get; set; }
        public bool HasAgeLimit { get; set; }
        public Guid? NumberGroupId { get; set; }
        public Championship? Championship { get; set; }
        public List<RiderRegistration>? RiderRegistrations { get; set; }
        public List<ClassRiderResult>? Results { get; set; }
        public List<ScheduleToClass>? ScheduleLinks { get; set; }
        public List<RoundRating>? RoundRatings { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}