using System;
using System.Collections.Generic;

namespace Cli.BraaapWebModel
{
    public class RiderProfile: ITimestamp
    {
        public string? City { get; set; }
        public string? FirstName { get; set; }
        public string? ParentName { get; set; }
        public string? LastName { get; set; }
        public int PreferredNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public bool IsActive { get; set; }
        /// <summary>
        /// User has been registered to a race and confirmed registration with passport
        /// </summary>
        public bool Confirmed { get; set; }
        public bool IsSeed { get; set; }
        public List<RiderRegistration>? Registrations { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}