using System;
using System.Collections.Generic;

namespace maxbl4.Race.Logic.EventModel
{
    public class Event: ITimestamp
    {
        public Guid EventId { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Reglament { get; set; }
        public string ResultsTemplate { get; set; }
        public Guid? ChampionshipId { get; set; }
        public Championship Championship { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public List<Schedule> Schedules { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTimeOffset StartOfRegistration { get; set; }
        public DateTimeOffset EndOfRegistration { get; set; }
        public Guid? TrackId { get; set; }
        public Track Track { get; set; }
    }

    public class EventPrice : ITimestamp
    {
        [System.ComponentModel.DataAnnotations.Key]
        public Guid EventId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal PaymentMultiplier { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    public class Track : ITimestamp
    {
        public Guid TrackId { get; set; }
        public string Name { get; set; }
        public string MapLink { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}