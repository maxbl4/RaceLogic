using System;
using System.Net.Http;
using System.Threading.Tasks;
using BraaapWeb.Client;
using FluentAssertions;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Tests.Infrastructure;
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
            upstreamDataSyncServiceOptions = new UpstreamDataSyncServiceOptions
            {
                BaseUri = "fake",
                ApiKey = "fake",
                StorageConnectionString = "TimingSessionServiceTests.litedb"
            };
        }
        
        [Fact]
        public async Task Create_and_start_timing_session()
        {
            var messageHub = new ChannelMessageHub();
            var upstreamDataStorage = new UpstreamDataStorageService(Options.Create(upstreamDataSyncServiceOptions));
            var upstreamDataSyncService = new UpstreamDataSyncService(Options.Create(upstreamDataSyncServiceOptions), new FakeMainClient(), 
                upstreamDataStorage, messageHub);
            var downloadResult = await upstreamDataSyncService.Download(true);
            downloadResult.Should().BeTrue();
            upstreamDataStorage.ListSeries().Should().HaveCount(4);
            var eventRepository = new LiteDbEventRepository(Options.Create(upstreamDataSyncServiceOptions), messageHub);
        }
    }
}