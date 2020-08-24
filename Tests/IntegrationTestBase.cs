using System;
using System.IO;
using System.Linq;
using System.Threading;
using AutoMapper;
using Easy.MessageHub;
using FluentAssertions;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.TestOutputHelperExt;
using maxbl4.Race.CheckpointService;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Tests.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using ServiceBase;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests
{
    public class IntegrationTestBase
    {
        public const string TestAdminWsToken = "test-admin-token";
        public const string WsToken1 = "ws-token-1";
        public const string WsToken2 = "ws-token-2";

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

        public void WithCheckpointStorageService(Action<StorageService> storageServiceInitializer)
        {
            Logger.Debug("Creating CheckpointStorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}), 
                MessageHub, SystemClock);
            storageServiceInitializer(storageService);
        }
        
        public void WithDataStorageService(Action<Race.DataService.Services.StorageService> storageServiceInitializer)
        {
            Logger.Debug("Creating DataStorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new Race.DataService.Services.StorageService(Options.Create(new Race.DataService.Options.ServiceOptions{StorageConnectionString = storageConnectionString}), 
                MessageHub, SystemClock);
            storageServiceInitializer(storageService);
        }
        
        public void WithRfidService(Action<StorageService, RfidService> action)
        {
            Logger.Debug("Creating CheckpointStorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}),
                MessageHub, SystemClock);
            using var rfidService = new RfidService(storageService, MessageHub, SystemClock, Mapper);
            action(storageService, rfidService);
        }
        
        public T WithCheckpointStorageService<T>(Func<StorageService, T> storageServiceInitializer)
        {
            Logger.Debug("Creating CheckpointStorageServiceService with {@storageConnectionString}", storageConnectionString);
            using var storageService = new StorageService(Options.Create(new ServiceOptions{StorageConnectionString = storageConnectionString}),
                MessageHub, SystemClock);
            return storageServiceInitializer(storageService);
        }
        
        public ServiceRunner<Startup> CreateCheckpointService(int pauseStartupMs = 0, int port = 0)
        {
            Logger.Debug("Creating CheckpointService with {@storageConnectionString}", storageConnectionString);
            var svc = new ServiceRunner<Startup>();
            svc.Start(new []
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}", 
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}",
                $"--Urls=http://127.0.0.1:{port}"
            }).Wait(0);
            return svc;
        }
        
        public ServiceRunner<maxbl4.Race.WsHub.Startup> CreateWsHubService()
        {
            Logger.Debug("Creating WsHubService");
            var svc = new ServiceRunner<maxbl4.Race.WsHub.Startup>();
            svc.Start(new []
            {
                $"--Urls=http://127.0.0.1:0",
                $"--ServiceOptions:InitialAdminTokens={TestAdminWsToken},{WsToken1},{WsToken2}"
            }).Wait(0);
            return svc;
        }

        public ServiceRunner<Race.DataService.Startup> CreateDataService(int pauseStartupMs = 0)
        {
            Logger.Debug("Creating CheckpointService with {@storageConnectionString}", storageConnectionString);
            var svc = new ServiceRunner<Race.DataService.Startup>();
            svc.Start(new []
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}", 
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}",
                $"--Urls=http://127.0.0.1:0"
            }).Wait(0);
            return svc;
        }
    }
}