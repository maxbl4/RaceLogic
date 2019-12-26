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
        private readonly IOptions<ServiceOptions> serviceOptions;
     
        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock) :
            base(serviceOptions.Value.StorageConnectionString, messageHub, systemClock)
        {
            this.serviceOptions = serviceOptions;
        }

        protected override void ValidateDatabase()
        {
        }

        protected override void SetupIndexes()
        {
        }

        public IEnumerable<BsonDocument> Search(string collectionName, string query, int limit)
        {
            return repo.Query<BsonDocument>(collectionName).Where(query).Limit(limit).ToDocuments();
        }
        
        public long Count(string collectionName, string query)
        {
            return repo.Query<BsonDocument>(collectionName).Where(query).LongCount();
        }
        
        public BsonValue Upsert(string collectionName, BsonValue document)
        {
            repo.Upsert(document, collectionName);
            return document["_id"];
        }
    }
}