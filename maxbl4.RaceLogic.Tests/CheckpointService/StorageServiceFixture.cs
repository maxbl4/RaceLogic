using System;
using System.IO;
using LiteDB;
using maxbl4.RfidCheckpointService.Services;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class StorageServiceFixture : IDisposable
    {
        protected readonly StorageService storageService;
        protected readonly ConnectionString connectionString;

        public StorageServiceFixture()
        {
            connectionString = new ConnectionString
            {
                Filename = $"{Guid.NewGuid():N}.litedb",
                UtcDate = true
            };
            
            storageService = new StorageService(connectionString);
        }
        
        public void Dispose()
        {
            if (File.Exists(connectionString.Filename))
                File.Delete(connectionString.Filename);
        }
    }
}