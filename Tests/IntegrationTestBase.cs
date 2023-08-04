using System;
using System.Threading;
using AutoMapper;
using LiteDB;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.CheckpointService;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Tests.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests
{
    public class IntegrationTestBase
    {
        public const string TestAdminWsToken = "test-admin-token";
        public const string WsToken1 = "ws-token-1";
        public const string WsToken2 = "ws-token-2";

        private static readonly object sync = new();
        private readonly ThreadLocal<IMessageHub> messageHub = new(() => new ChannelMessageHub());
        protected readonly string storageConnectionString;
        protected readonly FakeSystemClock SystemClock = new();

        public IntegrationTestBase(ITestOutputHelper outputHelper)
        {
            lock (sync)
            {
                ServiceRunner<Startup>.SetupLogger("testsettings");
            }

            Logger = Log.ForContext(GetType());
            BsonMapper.Global.RegisterIdBsonMappers();
            BsonMapper.Global.RegisterIdBsonMappers(GetType().Assembly);
            storageConnectionString = outputHelper.GetEmptyLiteDbForTest();
            Logger.Debug("Storage {@connectionString}", storageConnectionString);
            Mapper = new MapperConfiguration(x => x.AddMaps(typeof(Startup)))
                .CreateMapper();
        }

        protected IMessageHub MessageHub => messageHub.Value;
        protected ILogger Logger { get; }
        protected IMapper Mapper { get; }

        public void WithCheckpointStorageService(Action<CheckpointRepository> storageServiceInitializer)
        {
            Logger.Debug("Creating CheckpointStorageServiceService with {@storageConnectionString}",
                storageConnectionString);
            
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var repo = new CheckpointRepository(Options.Create(new ServiceOptions()), storageService, MessageHub, SystemClock);
            storageServiceInitializer(repo);
        }

        public void WithDataStorageService(Action<DataServiceRepository> storageServiceInitializer)
        {
            Logger.Debug("Creating DataStorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var repo = new DataServiceRepository(storageService);
            storageServiceInitializer(repo);
        }
        
        public void WithEventRepository(Action<StorageService, EventRepository> action)
        {
            Logger.Debug("Creating EventRepository with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var upstreamRepo = new UpstreamDataRepository(storageService);
            var eventRepo = new EventRepository(storageService, upstreamRepo);
            action(storageService, eventRepo);
        }

        public void WithRfidService(Action<CheckpointRepository, RfidService> action)
        {
            Logger.Debug("Creating CheckpointStorageServiceService with {@storageConnectionString}",
                storageConnectionString);
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var repo = new CheckpointRepository(Options.Create(new ServiceOptions()), storageService, MessageHub, SystemClock);
            using var rfidService = new RfidService(repo, MessageHub, SystemClock, Mapper);
            action(repo, rfidService);
        }

        public T WithCheckpointStorageService<T>(Func<CheckpointRepository, T> storageServiceInitializer)
        {
            Logger.Debug("Creating CheckpointStorageServiceService with {@storageConnectionString}",
                storageConnectionString);
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var repo = new CheckpointRepository(Options.Create(new ServiceOptions()), storageService, MessageHub, SystemClock);
            return storageServiceInitializer(repo);
        }

        public ServiceRunner<Startup> CreateCheckpointService(int pauseStartupMs = 0, int port = 0)
        {
            Logger.Debug("Creating CheckpointService with {@storageConnectionString}", storageConnectionString);
            var svc = new ServiceRunner<Startup>();
            svc.Start(new[]
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}",
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}",
                $"--Urls=http://127.0.0.1:{port}"
            }).Wait(0);
            return svc;
        }

        public ServiceRunner<Race.WsHub.Startup> CreateWsHubService()
        {
            Logger.Debug("Creating WsHubService with {@storageConnectionString}", storageConnectionString);
            var svc = new ServiceRunner<Race.WsHub.Startup>();
            svc.Start(new[]
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}",
                "--Urls=http://127.0.0.1:0",
                $"--ServiceOptions:InitialAdminTokens={TestAdminWsToken},{WsToken1},{WsToken2}"
            }).Wait(0);
            return svc;
        }

        public ServiceRunner<Race.DataService.Startup> CreateDataService(int pauseStartupMs = 0)
        {
            Logger.Debug("Creating CheckpointService with {@storageConnectionString}", storageConnectionString);
            var svc = new ServiceRunner<Race.DataService.Startup>();
            svc.Start(new[]
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}",
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}",
                "--Urls=http://127.0.0.1:0"
            }).Wait(0);
            return svc;
        }
    }
}