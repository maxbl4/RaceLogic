﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.PlatformServices;
using maxbl4.Infrastructure;
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
            var sessionsToResume = eventRepository
                .ListStoredActiveTimingSessions().ToList();
            foreach (var dto in sessionsToResume)
            {
                ResumeSession(dto);
            }
            messageHub.Publish(new ActiveTimingSessionsUpdate{Sessions = sessionsToResume});
        }

        public void ResumeSession(Id<TimingSessionDto> id)
        {
            using var _ = sync.Use();
            ResumeSession(eventRepository.StorageService.Get(id));
            
            ListActiveTimingSessions();
        }

        private void ResumeSession(TimingSessionDto dto)
        {
            var activeSession = activeSessions
                .FirstOrDefault(x => x.Id == dto.Id);
            if (activeSession != null) return;
            dto.IsRunning = true;
            eventRepository.StorageService.Save(dto);
            
            var newSession = new TimingSession(dto.Id, dto.SessionId, checkpointStorage, eventRepository, messageHub, autoMapperProvider);
            newSession.Start();
            activeSessions.Add(newSession);

            var rating = newSession.GetTimingSessionUpdate();
            eventRepository.StorageService.Save(rating);
            messageHub.Publish(rating);
        }

        public void StopSession(Id<TimingSessionDto> id)
        {
            using var _ = sync.Use();
            var activeSession = activeSessions
                .FirstOrDefault(x => x.Id == id);
            if (activeSession == null) return;
            StopSession(activeSession.SessionId);
        }

        public TimingSessionUpdate GetTimingSessionRating(Id<TimingSessionDto> id, bool forceUpdate = false)
        {
            using var _ = sync.Use();
            TimingSessionUpdate rating;
            
            var activeSession = activeSessions
                .FirstOrDefault(x => x.Id == id);
            if (activeSession != null)
            {
                rating = activeSession.GetTimingSessionUpdate();
            }
            else
            {
                rating = eventRepository.StorageService.Get<TimingSessionUpdate>(id.Value);
                if (!forceUpdate && rating != null)
                {
                    messageHub.Publish(rating);
                    return rating;
                }
                
                var ts = eventRepository.StorageService.Get(id);
                var newSession = new TimingSession(id, ts.SessionId, checkpointStorage,
                    eventRepository, messageHub, autoMapperProvider);
                newSession.Reload(false);
                rating = newSession.GetTimingSessionUpdate();
            }
            
            eventRepository.StorageService.Save(rating);
            messageHub.Publish(rating);
            return rating;
        }

        public List<TimingSessionDto> ListActiveTimingSessions()
        {
            using var _ = sync.Use();
            var s = activeSessions
                .Select(x => eventRepository.StorageService.Get(x.Id))
                .ToList();
            messageHub.Publish(new ActiveTimingSessionsUpdate{Sessions = s});
            return s;
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
            var rating = activeSession.GetTimingSessionUpdate();
            eventRepository.StorageService.Save(rating);
            messageHub.Publish(rating);
            activeSessions.Remove(activeSession);
            activeSession.DisposeSafe();
            
            ListActiveTimingSessions();
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
            newSession.Start();

            activeSessions.Add(newSession);
            
            ListActiveTimingSessions();
            return newSession.Id;
        }

        public void Dispose()
        {
            disposable.DisposeSafe();
        }
    }
}