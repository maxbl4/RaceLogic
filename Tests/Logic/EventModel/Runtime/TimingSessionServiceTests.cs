using System.Net.Http;
using System.Threading.Tasks;
using BraaapWeb.Client;
using FluentAssertions;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.UpstreamData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace maxbl4.Race.Tests.Logic.EventModel.Runtime
{
    public class TimingSessionServiceTests
    {
        private readonly UpstreamDataSyncServiceOptions upstreamDataSyncServiceOptions;

        public TimingSessionServiceTests()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<TimingSessionServiceTests>()
                .Build();
            upstreamDataSyncServiceOptions = config.GetSection(nameof(UpstreamDataSyncServiceOptions)).Get<UpstreamDataSyncServiceOptions>();
            upstreamDataSyncServiceOptions.StorageConnectionString = "TimingSessionServiceTests.litedb";
        }

        [Fact]
        public void Secrets_loaded()
        {
            upstreamDataSyncServiceOptions.ApiKey.Should().NotBeEmpty();
            upstreamDataSyncServiceOptions.BaseUri.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task Create_and_start_timing_session()
        {
            var messageHub = new ChannelMessageHub();
            var upstreamDataStorage = new UpstreamDataStorageService(Options.Create(upstreamDataSyncServiceOptions));
            var mainClient = new MainClient(upstreamDataSyncServiceOptions.BaseUri, new HttpClient());
            var upstreamDataSyncService = new UpstreamDataSyncService(Options.Create(upstreamDataSyncServiceOptions), mainClient, 
                upstreamDataStorage, messageHub);
            var downloadResult = await upstreamDataSyncService.Download(true);
            downloadResult.Should().BeTrue();
            var eventRepository = new LiteDbEventRepository(Options.Create(upstreamDataSyncServiceOptions), messageHub);
            //TimingSessionService timingSessionService = new();

        }
    }
}