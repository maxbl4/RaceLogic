﻿using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Storage.Model
{
    public class RiderClassRegistrationDto : IHasId<RiderClassRegistrationDto>, IHasTimestamp, IHasSeed, IHasPersonName
    {
        public Id<RiderProfileDto> RiderProfileId { get; set; }
        public Id<ClassDto> ClassId { get; set; }
        public Id<ChampionshipDto> ChampionshipDtoId { get; set; }
        public DateTime Birthdate { get; set; }
        public Sex Sex { get; set; }
        public string RiderDescription { get; set; }
        public bool IdentityConfirmed { get; set; }
        public DateTime IdentityConfirmedDate { get; set; }

        public string FirstName { get; set; }
        public string ParentName { get; set; }
        public string LastName { get; set; }
        public string Moto { get; set; }
        public int Number { get; set; }
        public bool Validated { get; set; }
        public DateTime ValidatedDate { get; set; }
        public bool IsDisqualified { get; set; }
        public Id<RiderClassRegistrationDto> Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public override string ToString()
        {
            if (FirstName != null || LastName != null)
                return string.Join(" ", LastName, FirstName);
            return Id.ToString();
        }
    }
    
    public class RiderEventInfoDto : IHasPersonName
    {
        public Id<RiderClassRegistrationDto> Id { get; set; }
        public Id<RiderProfileDto> RiderProfileId { get; set; }
        public Id<ClassDto> ClassId { get; set; }
        public string FirstName { get; set; }
        public string ParentName { get; set; }
        public string LastName { get; set; }
        public string ClassName { get; set; }
        public int Number { get; set; }
        public bool IsDisqualified { get; set; }

        public RiderEventInfoDto SetClassName(string name)
        {
            ClassName = name;
            return this;
        }
    }
}