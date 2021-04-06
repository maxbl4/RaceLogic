using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RiderEventRegistrationDto : IHasId<RiderEventRegistrationDto>, IHasTimestamp, IHasSeed
    {
        public Id<RiderClassRegistrationDto> RiderClassRegistrationId { get; set; }
        public Id<EventDto> EventId { get; set; }
        public Id<ClassDto> ClassId { get; set; }
        
        public bool Validated { get; set; }
        public DateTime ValidatedDate { get; set; }
        public decimal Paid { get; set; }
        public int OrdinalIndex { get; set; }
        public bool PaymentConfirmed { get; set; }
        public bool IsDisqualified { get; set; }
        public Id<RiderEventRegistrationDto> Id { get; set; }
        public HashSet<string> Identifiers { get; set; } = new();
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}