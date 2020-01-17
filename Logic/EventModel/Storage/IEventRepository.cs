﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public interface IEventRepository
    {
        List<T> GetRawDto<T, K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, K>> orderBy = null,
            int? skip = null, int? limit = null);

        Id<T> Save<T>(T recordingSession)
            where T: IHasId<T>;
    }
}