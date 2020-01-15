using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class RiderProfile: IHasTimestamp
    {
        public string Description { get; set; }
        public string FirstName { get; set; }
        public string ParentName { get; set; }
        public string LastName { get; set; }
        public int PreferredNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public bool IsActive { get; set; }
        public bool IdentityConfirmed { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}