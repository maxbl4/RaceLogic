using System.Collections.Generic;
using System.Reactive.PlatformServices;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.WsHub.Model;
using Microsoft.Extensions.Options;
using ServiceBase;

namespace maxbl4.Race.WsHub.Services
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
            repo.Query<AuthToken>().OrderBy(x => x.Token).FirstOrDefault();
        }

        protected override void SetupIndexes()
        {
            repo.Database.GetCollection<AuthToken>().EnsureIndex(x => x.Updated);
            repo.Database.GetCollection<AuthToken>().EnsureIndex(x => x.ServiceName);
        }

        public List<AuthToken> GetTokens()
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