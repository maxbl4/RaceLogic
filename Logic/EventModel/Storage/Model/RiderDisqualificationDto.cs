using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RiderDisqualificationDto : IHasId<RiderDisqualificationDto>, IHasTimestamp, IHasSeed
    {
        public string Reason { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid SessionId { get; set; }
        public Guid Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}