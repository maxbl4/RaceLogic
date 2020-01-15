using System;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class Tag: IHasTimestamp
    {
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration RiderRegistration { get; set; }
        public string TagId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}