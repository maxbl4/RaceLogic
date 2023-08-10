using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.PlatformServices;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.Extensions;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Logic.WebModel;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSessionService : ITimingSessionService, IDisposable
    {
        private readonly IAutoMapperProvider autoMapperProvider;
        private readonly ISystemClock clock;
        private readonly CheckpointRepository checkpointStorage;
        private readonly IEventRepository eventRepository;
        private readonly IMessageHub messageHub;
        private readonly SyncLock sync = new();
        private readonly List<TimingSession> activeSessions = new();
        private readonly CompositeDisposable disposable;

        public TimingSessionService(CheckpointRepository checkpointStorage, IEventRepository eventRepository, IMessageHub messageHub, 
            IAutoMapperProvider autoMapperProvider, ISystemClock clock)
        {
            this.checkpointStorage = checkpointStorage;
            this.eventRepository = eventRepository;
            this.messageHub = messageHub;
            this.autoMapperProvider = autoMapperProvider;
            this.clock = clock;
            disposable = new CompositeDisposable(
                    messageHub
                        .Subscribe<UpstreamDataSyncComplete>(ReloadActiveSessions));
        }

        private void ReloadActiveSessions(UpstreamDataSyncComplete obj)
        {
            using var _ = sync.Use();
            foreach (var activeSession in activeSessions)
            {
                activeSession.Reload();
            }
        }

        public void Initialize()
        {
            var sessionsToResume = eventRepository.ListStoredActiveTimingSessions();
            foreach (var dto in sessionsToResume)
            {
                ResumeSession(dto);
            }
        }

        public void ResumeSession(Id<TimingSessionDto> id)
        {
            using var _ = sync.Use();
            ResumeSession(eventRepository.StorageService.Get(id));
        }

        private void ResumeSession(TimingSessionDto dto)
        {
            var activeSession = activeSessions
                .FirstOrDefault(x => x.Id == dto.Id);
            if (activeSession != null) return;
            dto.IsRunning = true;
            eventRepository.StorageService.Save(dto);
            
            var newSession = new TimingSession(dto.Id, dto.SessionId, checkpointStorage, eventRepository, messageHub, autoMapperProvider);
            activeSessions.Add(newSession);
        }

        public void StopSession(Id<TimingSessionDto> id)
        {
            using var _ = sync.Use();
            var activeSession = activeSessions
                .FirstOrDefault(x => x.Id == id);
            if (activeSession == null) return;
            StopSession(activeSession.SessionId);
        }

        public TimingSessionUpdate GetTimingSessionRating(Id<TimingSessionDto> id)
        {
             var rating = eventRepository.StorageService.Get<TimingSessionUpdate>(id.Value);
             if (rating != null) return rating;
             var ts = eventRepository.StorageService.Get(id);
             var session = eventRepository.GetWithUpstream(ts.SessionId);

             var cpHandler = new TimingCheckpointHandler(ts.StartTime, id, session,
                 eventRepository.GetRiderIdentifiers(ts.SessionId));
             var cps = checkpointStorage.ListCheckpoints(ts.StartTime, ts.StopTime);
             foreach (var cp in cps)
             {
                 cpHandler.AppendCheckpoint(cp);
             }
             rating = TimingSessionUpdate.From(id, cpHandler.Track.Rating, autoMapperProvider);
             eventRepository.StorageService.Save(rating);
             return rating;
        }

        public List<TimingSessionDto> ListActiveTimingSessions()
        {
            using var _ = sync.Use();
            return activeSessions
                .Select(x => eventRepository.StorageService.Get(x.Id))
                .ToList();
        }

        public void StopSession(Id<SessionDto> sessionId)
        {
            using var _ = sync.Use();
            var activeSession = activeSessions
                .FirstOrDefault(x => x.SessionId == sessionId);
            if (activeSession == null) return;
            
            eventRepository.StorageService.Update(activeSession.Id, x =>
            {
                x.Stop(clock.UtcNow.UtcDateTime);
            });
            var rating = TimingSessionUpdate.From(activeSession.Id, activeSession.Rating, autoMapperProvider);
            eventRepository.StorageService.Save(rating);
            messageHub.Publish(rating);
            activeSessions.Remove(activeSession);
            activeSession.DisposeSafe();
        }

        public Id<TimingSessionDto> StartNewSession(string name, Id<SessionDto> sessionId)
        {
            using var _ = sync.Use();
            var activeSession = activeSessions
                .FirstOrDefault(x => x.SessionId == sessionId);
            if (activeSession != null) return activeSession.Id;
            
            var session = eventRepository.GetWithUpstream(sessionId);
            var dto = new TimingSessionDto
            {
                Name = name,
                EventId = session.EventId,
                SessionId = sessionId
            };
            dto.Start(clock.UtcNow.UtcDateTime);
            eventRepository.StorageService.Save(dto);
            var newSession = new TimingSession(dto.Id, sessionId, checkpointStorage,
                eventRepository, messageHub, autoMapperProvider);

            activeSessions.Add(newSession);
            return newSession.Id;
        }

        public void Dispose()
        {
            disposable.DisposeSafe();
        }
    }
}