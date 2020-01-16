using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RiderDisqualificationDto: IHasId<RiderDisqualificationDto>, IHasTimestamp, IHasSeed
    {
        public Id<RiderDisqualificationDto> Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
        
        public string Reason { get; set; }
        public Id<RegistrationDto> RegistrationId { get; set; }
        public Id<SessionDto> SessionId { get; set; }
    }
}