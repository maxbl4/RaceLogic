using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public interface IEventRepository
    {
        List<T> GetRawDtos<T, K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, K>> orderBy = null,
            int? skip = null, int? limit = null)
            where T : IHasId<T>;

        Guid Save<T>(T recordingSession)
            where T : IHasId<T>;

        List<T> GetRawDtos<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null)
            where T : IHasId<T>;

        T GetRawDtoById<T>(Guid id)
            where T : IHasId<T>;

        IEnumerable<RegistrationDto> GetRegistrations(Guid classId, Guid eventId);
        List<RegistrationDto> GetRegistrations(Guid sessionId);
        List<RiderIdentifierDto> GetRiderIdentifiersBySession(Guid sessionId);
    }
}