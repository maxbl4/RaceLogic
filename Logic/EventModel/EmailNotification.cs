using System;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class EmailNotification : IHasTimestamp
    {
        public Guid EmailNotificationId { get; set; }
        
        public Guid RiderProfileId { get; set; }
        public RiderProfile RiderProfile { get; set; }
        public EmailNotificationTypes Type { get; set; }
        
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    public enum EmailNotificationTypes
    {
        NewRegistrationAdded,
        RegistrationUpdated
    }
}