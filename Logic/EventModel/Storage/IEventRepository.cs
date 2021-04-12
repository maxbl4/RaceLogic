﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public interface IEventRepository
    {
        List<T> GetRawDtos<T, K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, K>> orderBy = null,
            int? skip = null, int? limit = null)
            where T : IHasId<T>;

        Id<T> Save<T>(T entity)
            where T : IHasId<T>;

        List<T> GetRawDtos<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null)
            where T : IHasId<T>;

        T GetRawDtoById<T>(Id<T> id)
            where T : IHasId<T>;

        IEnumerable<RiderClassRegistrationDto> GetRegistrations(Id<ClassDto> classId, Id<EventDto> eventId);
        List<RiderClassRegistrationDto> GetRegistrations(Id<SessionDto> sessionId);
        Dictionary<string, List<Id<RiderClassRegistrationDto>>> GetRiderIdentifiers(Id<SessionDto> sessionId);
    }
}