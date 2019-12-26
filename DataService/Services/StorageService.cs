using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using LiteDB;
using maxbl4.Race.DataService.Options;
using Microsoft.Extensions.Options;
using ServiceBase;

namespace maxbl4.Race.DataService.Services
{
    public class StorageService : StorageServiceBase
    {
        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock) :
            base(serviceOptions.Value.StorageConnectionString, messageHub, systemClock)
        {
        }

        protected override void ValidateDatabase()
        {
        }

        protected override void SetupIndexes()
        {
        }

        public T Get<T>(BsonValue key, string collectionName = null)
        {
            return repo.Database.GetCollection<T>(collectionName).FindById(key);
        }
        
        public IEnumerable<BsonDocument> Search(string collectionName, string @where, string order, int limit)
        {
            var query = repo.Query<BsonDocument>(collectionName)
                .Where(@where);
            foreach (var ord in ParseOrder(order))
            {
                if (ord.desc)
                    query = query.OrderByDescending(BsonExpression.Create(ord.field));
                else
                    query = query.OrderBy(BsonExpression.Create(ord.field));
            }
            return query.Limit(limit).ToDocuments();
        }
        
        public long Count(string collectionName, string query)
        {
            return repo.Query<BsonDocument>(collectionName).Where(query).LongCount();
        }
        
        public bool Upsert(string collectionName, BsonDocument document)
        {
            return repo.Database.GetCollection(collectionName, GetAutoId(document)).Upsert(document);
        }

        public static BsonAutoId GetAutoId(BsonDocument document)
        {
            if (!document.TryGetValue("_id", out var id)) return BsonAutoId.Guid;
            return id.Type switch
            {
                BsonType.Int32 => BsonAutoId.Int32,
                BsonType.Int64 => BsonAutoId.Int64,
                BsonType.ObjectId => BsonAutoId.ObjectId,
                BsonType.Guid => BsonAutoId.Guid,
                _ => 0
            };
        }

        public static IEnumerable<(string field, bool desc)> ParseOrder(string order)
        {
            if (string.IsNullOrEmpty(order))
                yield break;
            var fields = order.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var f in fields)
            {
                var parts = f.Split(new []{' ', '\t', '+'}, StringSplitOptions.RemoveEmptyEntries);
                switch (parts.Length)
                {
                    case 0:
                        break;
                    case 1:
                        yield return (parts[0], false);
                        break;
                    default:
                        yield return (parts[0], "desc".Equals(parts[1], StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }
        }
    }
}