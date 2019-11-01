using System;
using System.IO;
using LiteDB;
using maxbl4.RfidCheckpointService.Services;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class StorageServiceFixture
    {
        protected readonly StorageService storageService;
        protected readonly ConnectionString storageConnectionString;

        public StorageServiceFixture()
        {
            storageConnectionString = new ConnectionString
            {
                Filename = $"{GetType().Name}.litedb",
                UtcDate = true
            };
            if (File.Exists(storageConnectionString.Filename))
                File.Delete(storageConnectionString.Filename);
            
            storageService = new StorageService(storageConnectionString);
        }
    }
}