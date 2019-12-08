using System;
using System.IO;
using System.Linq;
using System.Threading;
using AutoMapper;
using Easy.MessageHub;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.TestOutputHelperExt;
using maxbl4.Race.CheckpointService;
using maxbl4.Race.CheckpointService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.CheckpointService
{
    public class IntegrationTestBase
    {
        private const int CheckpointsPort = 5050;
        private const int DataPort = 5060;
        static object sync = new object();
        private readonly ThreadLocal<IMessageHub> messageHub = new ThreadLocal<IMessageHub>(() => new MessageHub());
        protected string CheckpointsUri => $"http://localhost:{CheckpointsPort}";
        protected string DataUri => $"http://localhost:{DataPort}";
        protected IMessageHub MessageHub => messageHub.Value;
        protected readonly string storageConnectionString;
        protected readonly FakeSystemClock SystemClock = new FakeSystemClock();
        protected ILogger Logger { get; }
        protected IMapper Mapper { get; }
        
        public IntegrationTestBase(ITestOutputHelper outputHelper)
        {
            lock (sync)
            {
                CheckpointServiceRunner.SetupLogger("testsettings");
            }
            Logger = Log.ForContext(this.GetType());
            
            var fileName = GetNameForDbFile(outputHelper);
            Logger.Debug("Storage {@fileName}", fileName);
            new RollingFileInfo(fileName).Delete();
            storageConnectionString = $"Filename={fileName};UtcDate=true";
            Mapper = new MapperConfiguration(x => x.AddMaps(typeof(Startup)))
                .CreateMapper();
        }

        public void WithStorageService(Action<StorageService> storageServiceInitializer)
        {
            Logger.Debug("Creating StorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), 
                MessageHub, SystemClock);
            storageServiceInitializer(storageService);
        }
        
        public void WithRfidService(Action<StorageService, RfidService> action)
        {
            Logger.Debug("Creating StorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}),
                MessageHub, SystemClock);
            using var rfidService = new RfidService(storageService, MessageHub, SystemClock, Mapper);
            action(storageService, rfidService);
        }
        
        public T WithStorageService<T>(Func<StorageService, T> storageServiceInitializer)
        {
            Logger.Debug("Creating StorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}),
                MessageHub, SystemClock);
            return storageServiceInitializer(storageService);
        }
        
        public CheckpointServiceRunner CreateCheckpointService(int pauseStartupMs = 0)
        {
            Logger.Debug("Creating CheckpointService with {@storageConnectionString}", storageConnectionString);
            var svc = new CheckpointServiceRunner();
            svc.Start(new []
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}", 
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}",
                $"--Urls={CheckpointsUri}"
            }).Wait(0);
            return svc;
        }

        string GetNameForDbFile(ITestOutputHelper outputHelper)
        {
            Directory.CreateDirectory("var/data");
            var parts = outputHelper.GetTest().DisplayName.Split(".");
            return "var/data/" + "_" + string.Join("-", parts.Skip(Math.Max(parts.Length - 2, 0))) + ".litedb";
        }
    }
}