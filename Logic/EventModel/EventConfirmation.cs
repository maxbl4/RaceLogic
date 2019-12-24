using System;
using System.ComponentModel.DataAnnotations;

namespace maxbl4.Race.Logic.EventModel
{
    public class EventConfirmation: ITimestamp
    {
        [Key]
        public Guid EventConfirmationId { get; set; }
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration RiderRegistration { get; set; }
        public Guid EventId { get; set; }
        public Event Event { get; set; }
        public decimal Paid { get; set; }
        // Порядковый номер регистрации на событие. Используется для расчёта цены регистрации
        public int OrdinalIndex { get; set; }
        public bool PaymentConfirmed { get; set; }
        public bool WillAttend { get; set; }
        public bool TechControlPassed { get; set; }
        public bool MotoNumberPassed { get; set; }
        public bool SafetyEquipmentPassed { get; set; }
        public bool InsurancePassed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Validated { get; set; }
        public DateTimeOffset? ValidatedDate { get; set; }
    }
}