using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RegistrationDto : IHasId<RegistrationDto>, IHasTimestamp, IHasSeed
    {
        public Guid RiderProfileId { get; set; }
        public Guid EventId { get; set; }
        public Guid ClassId { get; set; }
        public string RiderDescription { get; set; }
        public string Moto { get; set; }
        public int Number { get; set; }
        public bool Validated { get; set; }
        public DateTime ValidatedDate { get; set; }
        public decimal Paid { get; set; }
        public int OrdinalIndex { get; set; }
        public bool PaymentConfirmed { get; set; }
        public Guid Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}