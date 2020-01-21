using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public class LiteDbEventRepository : IEventRepository
    {
        private readonly LiteRepository repo;

        public LiteDbEventRepository(LiteRepository repo)
        {
            this.repo = repo;
        }
        
        public T GetRawDtoById<T>(Id<T> id)
            where T: IHasId<T>
        {
            return repo.FirstOrDefault<T>(x => x.Id == id);
        }

        public List<T> GetRawDtos<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null)
            where T: IHasId<T>
        {
            return GetRawDtos<T, object>(predicate, null, skip, limit);
        }
        
        public List<T> GetRawDtos<T,K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T,K>> orderBy = null, int? skip = null, int? limit = null)
            where T: IHasId<T>
        {
            var query = repo.Query<T>();
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = query.OrderBy(orderBy);
            if (skip != null || limit != null)
            {
                ILiteQueryableResult<T> result = null;
                if (skip != null) result = query.Skip(skip.Value);
                if (limit != null) result = query.Limit(limit.Value);
                return result.ToList();
            }
            return query.ToList();
        }

        public Id<T> Save<T>(T recordingSession) where T : IHasId<T>
        {
            repo.Upsert(recordingSession.ApplyTraits());
            return recordingSession.Id;
        }

        public IEnumerable<RegistrationDto> GetRegistrations(Id<ClassDto> classId, Id<EventDto> eventId)
        {
            return repo.Query<RegistrationDto>().Where(x => x.ClassId == classId && x.EventId == eventId).ToEnumerable();
        }
        
        public List<RegistrationDto> GetRegistrations(Id<SessionDto> sessionId)
        {
            var session = GetRawDtoById(sessionId);
            if (session == null) return new List<RegistrationDto>();
            return session.ClassIds.Select(classId =>
                GetRegistrations(classId, session.EventId))
                .SelectMany(x => x)
                .ToList();
        }

        public IEnumerable<RiderIdentifierDto> GetRiderIdentifiers(Id<RiderProfileDto> riderProfileId)
        {
            return repo.Query<RiderIdentifierDto>().Where(x => x.RiderProfileId == riderProfileId).ToEnumerable();
        }
        
        public List<RiderIdentifierDto> GetRiderIdentifiers(Id<SessionDto> sessionId)
        {
            return GetRegistrations(sessionId).Select(x => GetRiderIdentifiers(x.RiderProfileId))
                .SelectMany(x => x).ToList();
        }
    }
}