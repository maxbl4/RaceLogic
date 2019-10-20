using System;
using System.IO;
using LiteDB;
using maxbl4.RfidCheckpointService.Services;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class StorageServiceFixture
    {
        protected readonly StorageService storageService;
        protected readonly ConnectionString connectionString;

        public StorageServiceFixture()
        {
            connectionString = new ConnectionString
            {
                Filename = $"{GetType().Name}.litedb",
                UtcDate = true
            };
            if (File.Exists(connectionString.Filename))
                File.Delete(connectionString.Filename);
            
            storageService = new StorageService(connectionString);
        }
    }
}