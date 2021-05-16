using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class RecordingService : IRecordingService, IDisposable
    {
        private static readonly ILogger logger = Log.ForContext<RecordingService>();
        private readonly IOptions<RecordingServiceOptions> options;
        private readonly IRecordingServiceRepository repository;
        private readonly IEventRepository eventRepository;
        private readonly ICheckpointServiceClientFactory checkpointServiceClientFactory;
        private readonly IAutoMapperProvider mapper;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock clock;
        private readonly SemaphoreSlim sync = new(1);
        private bool isRunning = true;
        private readonly Dictionary<Id<GateDto>, RfidSession> rfidSessions = new(); 

        public RecordingService(IOptions<RecordingServiceOptions> options, 
            IRecordingServiceRepository repository, IEventRepository eventRepository, ICheckpointServiceClientFactory checkpointServiceClientFactory, 
            IAutoMapperProvider mapper, IMessageHub messageHub, ISystemClock clock)
        {
            this.options = options;
            this.repository = repository;
            this.eventRepository = eventRepository;
            this.checkpointServiceClientFactory = checkpointServiceClientFactory;
            this.mapper = mapper;
            this.messageHub = messageHub;
            this.clock = clock;
        }

        async Task Heartbeat()
        {
            while (isRunning)
            {
                using var _ = sync.UseOnce();
                try
                {
                    foreach (var session in repository.GetActiveSessions())
                    {
                        repository.SaveSession(session);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                catch (Exception ex)
                {
                    logger.Warning("Heartbeat error {ex}", ex);
                }
            }
        }

        public void Initialize()
        {
            using var _ = sync.UseOnce();
            logger.Information("Initialize");
            var sessions = repository.GetActiveSessions();
            foreach (var session in sessions)
            {
                if (clock.UtcNow.UtcDateTime - session.Updated > TimeSpan.FromHours(2))
                {
                    logger.Information("Initialize stopping outdated session {id} from {time}", session.Id, session.Updated);
                    session.Stop(clock.UtcNow.UtcDateTime);
                    repository.SaveSession(session);
                }else
                {
                    logger.Information("Initialize restarting saved session {id}", session.Id);
                    if (session.UseRfid)
                    {
                        var gate = eventRepository.StorageService.Get(session.GateId);
                        StartRfidInt(session, gate);
                    }
                }
            }
            var __ = Heartbeat();
        }

        public Id<RecordingSessionDto> StartRfid(Id<GateDto> gateId)
        {
            using var _ = sync.UseOnce();
            var gate = eventRepository.StorageService.Get(gateId);
            if (gate == null)
                throw new KeyNotFoundException($"StartRfid gate {gateId} not found");
            var dto = repository.GetSessionForGate(gateId);
            if (!dto.IsRunning)
            {
                dto.Start(clock.UtcNow.UtcDateTime);
                dto.UseRfid = true;
                repository.SaveSession(dto);
            }
            StartRfidInt(dto, gate);
            return dto.Id;
        }

        public void StopRfid(Id<GateDto> gateId)
        {
            using var _ = sync.UseOnce();
            var gate = eventRepository.StorageService.Get(gateId);
            if (gate == null)
                throw new KeyNotFoundException($"StopRfid gate {gateId} not found");
            var dto = repository.GetSessionForGate(gateId);
            if (rfidSessions.TryGetValue(gateId, out var rfidSession))
            {
                logger.Information("StopRfid stopping rfid for gate {gateId}", gateId);
                rfidSession.Dispose();
                rfidSessions.Remove(gateId);
            }else
                logger.Warning("StopRfid rfid was not running gate {gateId}", gateId);
            if (dto.IsRunning)
            {
                logger.Information("StopRfid stopping {recordId} for gate {gateId}", dto.Id, gateId);
                dto.Stop(clock.UtcNow.UtcDateTime);
                repository.SaveSession(dto);
            }else
                logger.Warning("StopRfid {recordId} for gate {gateId} was not running", dto.Id, gateId);
        }

        private void StartRfidInt(RecordingSessionDto dto, GateDto gate)
        {
            if (rfidSessions.TryGetValue(gate.Id, out var rfidSession))
                rfidSession.Dispose();

            var client = checkpointServiceClientFactory.CreateClient(gate.CheckpointServiceAddress);
            rfidSessions[gate.Id] = rfidSession = new RfidSession
            {
                RecordingSession = dto
            };
            rfidSession.Disposable = new CompositeDisposable(
                rfidSession.Subscription = client.CreateSubscription(dto.StartTime),
                rfidSession.Subscription.Checkpoints.Subscribe(x => AppendCheckpoint(gate.Id, x))
            );
            rfidSession.Subscription.Start();
            client.SetRfidStatus(true);
        }

        public Id<RecordingSessionDto> AppendCheckpoint(Id<GateDto> gateId, Checkpoint checkpoint)
        {
            using var _ = sync.UseOnce();
            logger.Information("OnCheckpoint {riderId} {timestamp}", checkpoint.RiderId, checkpoint.Timestamp);
            RecordingSessionDto dto;
            if (rfidSessions.TryGetValue(gateId, out var rfidSession))
                dto = rfidSession.RecordingSession;
            else
                dto = repository.GetSessionForGate(gateId);
            
            var cp = mapper.Map<CheckpointDto>(checkpoint);
            cp.RecordingSessionId = dto.Id;
            repository.UpsertCheckpoint(cp);
            messageHub.Publish(cp);
            return dto.Id;
        }

        public void Dispose()
        {
            using var _ = sync.UseOnce();
            isRunning = false;
            foreach (var rfidSession in rfidSessions.Values)
            {
                rfidSession.Disposable.DisposeSafe();
            }
        }

        class RfidSession: IDisposable
        {
            public CompositeDisposable Disposable { get; set; }
            public ICheckpointSubscription Subscription { get; set; }
            public Subject<Checkpoint> Checkpoints { get; } = new();
            public RecordingSessionDto RecordingSession { get; set; }

            public void Dispose()
            {
                Disposable?.DisposeSafe();
            }
        }
    }
}