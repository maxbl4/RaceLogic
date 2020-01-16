using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using maxbl4.Race.Logic.EventModel;

namespace maxbl4.Race.Logic.EventManagement
{
    public interface IEventRepository
    {
        IEnumerable<SeriesDto> GetSeries(Expression<Func<SeriesDto, bool>> predicate);
    }
}