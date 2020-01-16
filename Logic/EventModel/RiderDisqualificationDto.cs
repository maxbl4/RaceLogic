﻿using System;
 using maxbl4.Race.Logic.EventModel.Traits;

 namespace maxbl4.Race.Logic.EventModel
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