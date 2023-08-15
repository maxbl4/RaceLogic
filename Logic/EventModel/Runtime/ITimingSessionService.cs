using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.WebModel;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface ITimingSessionService
    {
        Id<TimingSessionDto> StartNewSession(string name, Id<SessionDto> sessionId);
        void Initialize();
        void StopSession(Id<SessionDto> sessionId);
        void StopSession(Id<TimingSessionDto> id);
        List<TimingSessionDto> ListActiveTimingSessions();
        void ResumeSession(Id<TimingSessionDto> id);
        TimingSessionUpdate GetTimingSessionRating(Id<TimingSessionDto> id, bool forceUpdate = false);
    }
}