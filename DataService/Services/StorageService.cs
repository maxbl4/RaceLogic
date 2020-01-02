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
        
        public bool Delete<T>(BsonValue key, string collectionName = null)
        {
            return repo.Database.GetCollection<T>(collectionName).Delete(key);
        }
        
        public IEnumerable<BsonDocument> Search(string collectionName, string where, string order, int limit)
        {
            var query = repo.Query<BsonDocument>(collectionName)
                .Where(where);
            if (TryParseOrder(order, out var ord))
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

        public static bool TryParseOrder(string order, out (string field, bool desc) result)
        {
            if (string.IsNullOrEmpty(order))
            {
                result = default;
                return false;
            }

            if (order.StartsWith('-'))
                result = (order.Substring(1), true);
            else
                result = (order, false);
            return true;
        }
    }
}