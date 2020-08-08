using System;
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
            var col = repo.Database.GetCollection(collectionName, GetAutoId(document, out var isDefault));
            if (isDefault)
                document.Remove("_id");
            return col.Upsert(document);
        }

        public static BsonAutoId GetAutoId(BsonDocument document, out bool isDefault)
        {
            if (!document.TryGetValue("_id", out var id))
            {
                isDefault = true;
                return BsonAutoId.Guid;
            }
            switch (id.Type)
            {
                case BsonType.Int32:
                    isDefault = id.AsInt32 == 0;
                    return BsonAutoId.Int32;
                case BsonType.Int64:
                    isDefault = id.AsInt64 == 0L;
                    return BsonAutoId.Int64;
                case BsonType.ObjectId:
                    isDefault = id.AsObjectId == ObjectId.Empty;
                    return BsonAutoId.ObjectId;
                case BsonType.Guid:
                    isDefault = id.AsGuid == Guid.Empty;
                    return BsonAutoId.Guid;
                default:
                    isDefault = false;
                    return 0;
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