using System;
using System.Linq;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Tests.CheckpointService.Client;
using maxbl4.Race.Tests.Infrastructure;
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
                ApiKey = "fake"
            };
        }
        
        [Fact]
        public async Task Create_and_start_timing_session()
        {
            var messageHub = new ChannelMessageHub();
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var upstreamDataStorage = new UpstreamDataRepository(storageService);
            var eventRepository = new EventRepository(storageService, upstreamDataStorage);
            var recordingRepository = new RecordingServiceRepository(storageService);
            var upstreamDataSyncService = new UpstreamDataSyncService(Options.Create(upstreamDataSyncServiceOptions), new FakeMainClient(), 
                upstreamDataStorage, messageHub);
            var downloadResult = await upstreamDataSyncService.Download(true);
            downloadResult.Should().BeTrue();
            upstreamDataStorage.ListSeries().Should().HaveCount(4);

            var tagSub = new FakeCheckpointSubscription();
            var cpf = Substitute.For<ICheckpointServiceClientFactory>();
            var cps = Substitute.For<ICheckpointServiceClient>();
            cpf.CreateClient(Arg.Any<string>()).Returns(cps);
            cps.CreateSubscription(Arg.Any<DateTime>()).Returns(tagSub);

            storageService.Repo.Query<CheckpointDto>().Count().Should().Be(0);

            var recordingService = new RecordingService(recordingRepository, cpf, new AutoMapperProvider(), new DefaultSystemClock());
            var recordingSession = recordingService.StartRecordingSession("My session", "cps address");
            tagSub.SendTags((1, "11"), (2, "12"));
            storageService.Repo.Query<CheckpointDto>().Count().Should().Be(2);
            recordingRepository.GetActiveRecordingSession().Should().NotBeNull();
            
            var timingSessionService = new TimingSessionService(eventRepository, recordingService, recordingRepository, MessageHub, new AutoMapperProvider(),
                new DefaultSystemClock());

            var ev = upstreamDataStorage.ListEvents().First(x => x.Name == "Тучково кантри 12.09.2020");
            var session = upstreamDataStorage.ListSessions(ev.Id).First(x => x.Name == "Эксперт и Опен");
            var timingSession = timingSessionService.CreateSession("timing sess", session.EventId, session.Id, recordingSession.Id);
            timingSession.Start(tagSub.Now.AddSeconds(-10));
            await Task.Delay(100);
            timingSession.Track.Rating.Should().HaveCount(2);
            timingSession.Track.Rating[0].RiderId.Should().Be("11");
            timingSession.Track.Rating[1].RiderId.Should().Be("12");

            recordingSession.Stop();
            recordingRepository.GetActiveRecordingSession().Should().BeNull();
        }
    }
}