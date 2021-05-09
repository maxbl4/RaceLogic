using System.Collections.Generic;
using LiteDB;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.WsHub.Model;

namespace maxbl4.Race.WsHub.Services
{
    public class WsHubRepository : IRepository
    {
        public WsHubRepository(IStorageService storageService)
        {
            StorageService = storageService;
        }

        public IStorageService StorageService { get; }

        void IRepository.ValidateDatabase(ILiteRepository repo)
        {
            repo.Query<AuthToken>().OrderBy(x => x.Token).FirstOrDefault();
        }

        void IRepository.SetupIndexes(ILiteRepository repo)
        {
            repo.Database.GetCollection<AuthToken>().EnsureIndex(x => x.Updated);
            repo.Database.GetCollection<AuthToken>().EnsureIndex(x => x.ServiceName);
        }

        public List<AuthToken> GetTokens()
        {
            return StorageService.Repo.Query<AuthToken>().ToList();
        }

        public bool UpsertToken(AuthToken token)
        {
            return StorageService.Repo.Upsert(token.ApplyTraits());
        }

        public bool DeleteToken(string token)
        {
            return StorageService.Repo.Delete<AuthToken>(token);
        }
    }
}