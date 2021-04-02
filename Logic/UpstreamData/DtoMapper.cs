using System;
using System.Collections.Generic;
using System.Linq;
using BraaapWeb.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.UpstreamData
{
    public static class DtoMapper
    {
        public static IEnumerable<SeriesDto> ToDto(this IEnumerable<Series> entities)
        {
            return entities.Select(ToDto);
        }
        
        public static SeriesDto ToDto(this Series entity)
        {
            return new SeriesDto
            {
                Id = entity.SeriesId,
                Name = entity.Name,
                Description = entity.Description,
                Published = entity.Published,
                IsSeed = entity.Seed,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
        
        public static IEnumerable<ChampionshipDto> ToDto(this IEnumerable<Championship> entities)
        {
            return entities.Select(ToDto);
        }

        public static ChampionshipDto ToDto(this Championship entity)
        {
            return new ChampionshipDto
            {
                Id = entity.ChampionshipId,
                SeriesId = entity.SeriesId,
                Name = entity.Name,
                Description = entity.Description,
                Published = entity.Published,
                IsSeed = entity.Seed,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
        
        public static IEnumerable<ClassDto> ToDto(this IEnumerable<Class> entities)
        {
            return entities.Select(ToDto);
        }

        public static ClassDto ToDto(this Class entity)
        {
            return new ClassDto
            {
                Id = entity.ClassId,
                ChampionshipId = entity.ChampionshipId,
                NumberGroupId = entity.NumberGroupId,
                Name = entity.Name,
                Description = entity.Description,
                Published = entity.Published,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
        
        public static IEnumerable<EventDto> ToDto(this IEnumerable<Event> entities, 
            IEnumerable<EventPrice> eventPrices = null)
        {
            var joined = entities.GroupJoin(
                eventPrices ?? Array.Empty<EventPrice>(), 
                e => e.EventId, 
                p => p.EventId, 
                (e, p) => new {Event = e, Price = p.DefaultIfEmpty().FirstOrDefault()});
            return joined.Select(j => ToDto(j.Event, j.Price));
        }

        public static EventDto ToDto(this Event entity, EventPrice eventPrice = null)
        {
            return new EventDto
            {
                Id = entity.EventId,
                ChampionshipId = entity.ChampionshipId,
                Name = entity.Name,
                Description = entity.Description,
                Date = entity.Date,
                Regulations = entity.Reglament,
                BasePrice = eventPrice?.BasePrice ?? 0,
                PaymentMultiplier = eventPrice?.PaymentMultiplier ?? 0,
                ResultsTemplate = entity.ResultsTemplate,
                StartOfRegistration = entity.StartOfRegistration.UtcDateTime,
                EndOfRegistration = entity.EndOfRegistration.UtcDateTime,
                IsSeed = entity.Seed,
                Published = entity.Published,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
        
        public static IEnumerable<SessionDto> ToDto(this IEnumerable<Schedule> entities, 
            IEnumerable<ScheduleToClass> classes = null)
        {
            var joined = entities.GroupJoin(
                classes ?? Array.Empty<ScheduleToClass>(), 
                e => e.ScheduleId, 
                p => p.ScheduleId, 
                (e, p) => new {Schedule = e, ClassIds = p.Select(x => x.ClassId).ToList()});
            return joined.Select(j => ToDto(j.Schedule, j.ClassIds));
        }

        public static SessionDto ToDto(this Schedule entity, IEnumerable<Guid> classIds = null)
        {
            return new SessionDto
            {
                Id = entity.ScheduleId,
                EventId = entity.EventId,
                Name = entity.Name,
                Description = entity.Description,
                StartTime = entity.StartTime.UtcDateTime,
                FinishCriteria = new FinishCriteriaDto
                {
                    Duration = entity.Duration,
                } ,
                ClassIds = classIds?.Select(x => new Id<ClassDto>(x)).ToList() ?? new List<Id<ClassDto>>(),
                MinLap = entity.MinLap ?? TimeSpan.Zero,
                IsSeed = entity.Seed,
                Published = entity.Published,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
        
        public static IEnumerable<RiderProfileDto> ToDto(this IEnumerable<RiderProfile> entities)
        {
            return entities.Select(ToDto);
        }

        public static RiderProfileDto ToDto(this RiderProfile entity)
        {
            return new RiderProfileDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                ParentName = entity.ParentName,
                LastName = entity.LastName,
                RiderDescription = entity.City,
                Birthdate = entity.Birthdate.UtcDateTime,
                IdentityConfirmed = entity.Confirmed,
                IdentityConfirmedDate = entity.Updated.UtcDateTime,
                IsActive = entity.IsActive,
                Sex = Sex.NotSet,
                PreferredNumber = entity.PreferredNumber,
                IsSeed = entity.Seed,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
    }
}