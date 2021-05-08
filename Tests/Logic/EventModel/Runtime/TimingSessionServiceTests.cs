using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using BraaapWeb.Client;
using FluentAssertions;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Tests.CheckpointService.Client;
using maxbl4.Race.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.Logic.EventModel.Runtime
{
    public class TimingSessionServiceTests: IntegrationTestBase
    {
        private readonly UpstreamDataSyncServiceOptions upstreamDataSyncServiceOptions;
        public TimingSessionServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            upstreamDataSyncServiceOptions = new UpstreamDataSyncServiceOptions
            {
                BaseUri = "fake",
                ApiKey = "fake",
                StorageConnectionString = storageConnectionString
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

            var tagSub = new FakeCheckpointSubscription();
            var cpf = Substitute.For<ICheckpointServiceClientFactory>();
            var cps = Substitute.For<ICheckpointServiceClient>();
            cpf.CreateClient(Arg.Any<string>()).Returns(cps);
            cps.CreateSubscription(Arg.Any<DateTime>()).Returns(tagSub);

            eventRepository.Repo.Query<CheckpointDto>().Count().Should().Be(0);

            var recordingService = new RecordingService(eventRepository, cpf, new AutoMapperProvider(), new DefaultSystemClock());
            var recordingSession = recordingService.StartRecordingSession("My session", "cps address");
            tagSub.SendTags((1, "11"), (2, "12"));
            eventRepository.Repo.Query<CheckpointDto>().Count().Should().Be(2);
            eventRepository.GetActiveRecordingSession().Should().NotBeNull();
            
            var timingSessionService = new TimingSessionService(eventRepository, recordingService, MessageHub, new AutoMapperProvider(),
                new DefaultSystemClock());

            var ev = upstreamDataStorage.ListEvents().First(x => x.Name == "Тучково кантри 12.09.2020");
            var session = upstreamDataStorage.ListSessions(ev.Id).First(x => x.Name == "Эксперт и Опен");
            eventRepository.Save(session);
            var timingSession = timingSessionService.CreateSession("timing sess", session.EventId, session.Id, recordingSession.Id);
            timingSession.Start(tagSub.Now.AddSeconds(-10));
            await Task.Delay(100);
            timingSession.Track.Rating.Should().HaveCount(2);
            timingSession.Track.Rating[0].RiderId.Should().Be("11");
            timingSession.Track.Rating[1].RiderId.Should().Be("12");

            recordingSession.Stop();
            eventRepository.GetActiveRecordingSession().Should().BeNull();
        }
    }
}