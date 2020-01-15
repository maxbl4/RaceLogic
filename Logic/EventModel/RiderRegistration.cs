using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class RiderRegistration: IHasTimestamp
    {
        public Guid RiderRegistrationId { get; set; }
        public Guid RiderProfileId { get; set; }
        public RiderProfile RiderProfile { get; set; }
        public Guid ClassId { get; set; }
        public ClassDef Class { get; set; }
        public string Moto { get; set; }
        public string TeamName { get; set; }
        public int Number { get; set; }
        public List<EventConfirmation> EventConfirmations { get; set; }
        public bool Validated { get; set; }
        public DateTimeOffset? ValidatedDate { get; set; }
        public string TagSimple { get; set; }
        public List<RoundRiderResult> RoundRiderResults { get; set; }
        public List<RiderDisqualification> RiderDisqualifications { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}