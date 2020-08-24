using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.WsHub.Models;
using Microsoft.Extensions.Options;
using ServiceBase;

namespace maxbl4.Race.WsHub.Services
{
    public class StorageService : StorageServiceBase
    {
        private readonly IOptions<ServiceOptions> serviceOptions;

        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock): 
            base(serviceOptions.Value.StorageConnectionString, messageHub, systemClock)
        {
            this.serviceOptions = serviceOptions;
        }

        protected override void ValidateDatabase()
        {
            repo.Query<AuthToken>().OrderBy(x => x.Token).FirstOrDefault();
        }

        protected override void SetupIndexes()
        {
            repo.Database.GetCollection<AuthToken>().EnsureIndex(x => x.Updated);
            repo.Database.GetCollection<AuthToken>().EnsureIndex(x => x.ServiceName);
        }
        
        public List<AuthToken> ListTokens()
        {
            return repo.Query<AuthToken>().ToList();
        }
        
        public bool UpsertToken(AuthToken token)
        {
            return repo.Upsert(token.ApplyTraits());
        }
        
        public bool DeleteToken(string token)
        {
            return repo.Delete<AuthToken>(token);
        }
    }
}