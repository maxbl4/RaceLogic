using System;

namespace Cli.BraaapWebModel
{
    public class EmailNotification : ITimestamp
    {
        public Guid EmailNotificationId { get; set; }
        
        public Guid RiderProfileId { get; set; }
        public RiderProfile? RiderProfile { get; set; }
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