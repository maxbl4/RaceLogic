using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    // Rider profile is not a user, it can be created in the system, without a user to manage it (admins can manage it)
    // Rider profile created by user are managed by him
    public class RiderProfileDto : IHasId<RiderProfileDto>, IHasTimestamp, IHasSeed, IHasPersonName
    {
        public int PreferredNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public Sex Sex { get; set; }
        public string RiderDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IdentityConfirmed { get; set; }
        public DateTime IdentityConfirmedDate { get; set; }
        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string ParentName { get; set; }
        public string LastName { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    public enum Sex
    {
        NotSet,
        Male,
        Female
    }
}