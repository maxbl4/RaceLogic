using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RegistrationDto: IHasId<RegistrationDto>, IHasTimestamp, IHasSeed
    {
        public Id<RegistrationDto> Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        
        public Id<RiderProfileDto> RiderProfileId { get; set; }
        public Id<EventDto> EventId { get; set; }
        public Id<ClassDto> ClassId { get; set; }
        public string RiderDescription { get; set; }
        public string Moto { get; set; }
        public int Number { get; set; }
        public bool Validated { get; set; }
        public DateTime ValidatedDate { get; set; }
        public decimal Paid { get; set; }
        public int OrdinalIndex { get; set; }
        public bool PaymentConfirmed { get; set; }
    }
}