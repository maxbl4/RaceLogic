using System;
using System.IO;
using LiteDB;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class StorageServiceFixture
    {
        protected readonly StorageService storageService;
        protected readonly string storageConnectionString;
        
        public StorageServiceFixture()
        {
            var fileName = $"{GetType().Name}.litedb";
            if (File.Exists(fileName))
                File.Delete(fileName);
            storageConnectionString = $"Filename={fileName};UtcDate=true";
            
            storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), new NullLogger<StorageService>());
        }
    }
}