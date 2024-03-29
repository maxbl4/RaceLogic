using System;
using System.Linq;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Tests.CheckpointService.Client;
using maxbl4.Race.Tests.Infrastructure;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using SystemClock = System.Reactive.PlatformServices.SystemClock;

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
            var checkpointRepository = new CheckpointRepository(Options.Create(new ServiceOptions()), storageService, messageHub, SystemClock);
            var eventRepository = new EventRepository(storageService, upstreamDataStorage, new AutoMapperProvider());
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

            // using var recordingService = new RecordingService(Options.Create(new RecordingServiceOptions{CheckpointServiceAddress = "http://localhost:6000"}), recordingRepository, eventRepository, cpf, new AutoMapperProvider(), 
            //     messageHub, SystemClock);
            
            var timingSessionService = new TimingSessionService(checkpointRepository, eventRepository, MessageHub, new AutoMapperProvider(),
                new DefaultSystemClock());

            var ev = upstreamDataStorage.ListEvents().First(x => x.Name == "Тучково кантри 12.09.2020");
            
            var session = upstreamDataStorage.ListSessions(ev.Id).First(x => x.Name == "Эксперт и Опен");
            timingSessionService.StartNewSession("timing sess", session.Id);
            //var timingSession = timingSessionService.ActiveSession;
            //timingSession.Start(tagSub.Now.AddSeconds(-10));
            await Task.Delay(100);
            tagSub.SendTags((1, "11"), (2, "12"));
            await Task.Delay(100);
            storageService.Repo.Query<CheckpointDto>().Count().Should().Be(2);
            //recordingRepository.GetActiveRecordingSession().Should().NotBeNull();
            
            // timingSession.Track.Rating.Should().HaveCount(2);
            // timingSession.Track.Rating[0].RiderId.Should().Be("11");
            // timingSession.Track.Rating[1].RiderId.Should().Be("12");
            

            //recordingService.StopRecording();
            //recordingRepository.GetActiveRecordingSession().Should().BeNull();
        }
        
        [Fact]
        public async Task Create_and_start_timing_session2()
        {
            using var storageService = new StorageService(Options.Create(new StorageServiceOptions{StorageConnectionString = storageConnectionString}), MessageHub);
            var upstreamDataStorage = new UpstreamDataRepository(storageService);
            var checkpointRepository = new CheckpointRepository(Options.Create(new ServiceOptions()), storageService, MessageHub, SystemClock);
            var eventRepository = new EventRepository(storageService, upstreamDataStorage, new AutoMapperProvider());
            var upstreamDataSyncService = new UpstreamDataSyncService(Options.Create(upstreamDataSyncServiceOptions), new FakeMainClient(), 
                upstreamDataStorage, MessageHub);
            var downloadResult = await upstreamDataSyncService.Download(true);
            downloadResult.Should().BeTrue();
            upstreamDataStorage.ListSeries().Should().HaveCount(4);

            storageService.Repo.Query<CheckpointDto>().Count().Should().Be(0);
            
            var timingSessionService = new TimingSessionService(checkpointRepository, eventRepository, MessageHub, new AutoMapperProvider(),
                new DefaultSystemClock());

            var ev = upstreamDataStorage.ListEvents().First(x => x.Name == "Тучково кантри 12.09.2020");
            
            var session = upstreamDataStorage.ListSessions(ev.Id).First(x => x.Name == "Эксперт и Опен");
            var tsId = timingSessionService.StartNewSession("timing sess", session.Id);
            
            MessageHub.Publish(new Checkpoint("11", DateTime.UtcNow));
            await Task.Delay(100);

            var update = timingSessionService.GetTimingSessionRating(tsId);
            update.Rating.Should().HaveCount(1);
            
            //storageService.Repo.Query<Checkpoint>().Count().Should().Be(1);
        }
    }
}