using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public class LiteDbEventRepository : IEventRepository
    {
        private readonly LiteRepository repo;

        public LiteDbEventRepository(LiteRepository repo)
        {
            this.repo = repo;
        }
        
        public List<T> GetRawDto<T,K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T,K>> orderBy = null, int? skip = null, int? limit = null)
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
    }
}