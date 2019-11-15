using System;
using System.IO;
using System.Linq;
using LiteDB;
using maxbl4.RaceLogic.Tests.Ext;
using maxbl4.RfidCheckpointService;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class IntegrationTestBase
    {
        protected readonly string storageConnectionString;
        
        public IntegrationTestBase(ITestOutputHelper outputHelper)
        {
            var fileName = GetNameForDbFile(outputHelper);
            if (File.Exists(fileName))
                File.Delete(fileName);
            storageConnectionString = $"Filename={fileName};UtcDate=true";
        }

        public void WithStorageService(Action<StorageService> storageServiceInitializer)
        {
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), new NullLogger<StorageService>());
            storageServiceInitializer(storageService);
        }
        
        public T WithStorageService<T>(Func<StorageService, T> storageServiceInitializer)
        {
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), new NullLogger<StorageService>());
            return storageServiceInitializer(storageService);
        }
        
        public RfidCheckpointServiceRunner CreateRfidCheckpointService(int pauseStartupMs = 0)
        {
            var svc = new RfidCheckpointServiceRunner();
            svc.Start(new []
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}", 
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}"
            }).Wait(0);
            return svc;
        }

        string GetNameForDbFile(ITestOutputHelper outputHelper)
        {
            var parts = outputHelper.GetTest().DisplayName.Split(".");
            return string.Join("-", parts.Skip(Math.Max(parts.Length - 2, 0))) + ".litedb";
        }
    }
}