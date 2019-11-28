using System;
using System.IO;
using System.Linq;
using System.Threading;
using AutoMapper;
using Easy.MessageHub;
using maxbl4.RaceLogic.Tests.Ext;
using maxbl4.RfidCheckpointService;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class IntegrationTestBase
    {
        static object sync = new object();
        private readonly ThreadLocal<IMessageHub> messageHub = new ThreadLocal<IMessageHub>(() => new MessageHub());
        protected IMessageHub MessageHub => messageHub.Value;
        protected readonly string storageConnectionString;
        protected readonly FakeSystemClock SystemClock = new FakeSystemClock();
        protected ILogger Logger { get; }
        protected IMapper Mapper { get; }
        
        public IntegrationTestBase(ITestOutputHelper outputHelper)
        {
            lock (sync)
            {
                RfidCheckpointServiceRunner.SetupLogger("testsettings");
            }
            Logger = Log.ForContext(this.GetType());
            
            var fileName = GetNameForDbFile(outputHelper);
            Logger.Debug("Storage {@fileName}", fileName);
            if (File.Exists(fileName))
            {
                Logger.Debug("Remove existing {@filename}", fileName);
                File.Delete(fileName);
            }
            storageConnectionString = $"Filename={fileName};UtcDate=true";
            Mapper = new MapperConfiguration(x => x.AddMaps(typeof(Startup)))
                .CreateMapper();
        }

        public void WithStorageService(Action<StorageService> storageServiceInitializer)
        {
            Logger.Debug("Creating StorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub, SystemClock);
            storageServiceInitializer(storageService);
        }
        
        public void WithRfidService(Action<StorageService, RfidService> action)
        {
            Logger.Debug("Creating StorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub, SystemClock);
            using var rfidService = new RfidService(storageService, MessageHub, SystemClock, Mapper);
            action(storageService, rfidService);
        }
        
        public T WithStorageService<T>(Func<StorageService, T> storageServiceInitializer)
        {
            Logger.Debug("Creating StorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub, SystemClock);
            return storageServiceInitializer(storageService);
        }
        
        public RfidCheckpointServiceRunner CreateRfidCheckpointService(int pauseStartupMs = 0)
        {
            Logger.Debug("Creating RfidCheckpointService with {@storageConnectionString}", storageConnectionString);
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
            return "_" + string.Join("-", parts.Skip(Math.Max(parts.Length - 2, 0))) + ".litedb";
        }
    }
}