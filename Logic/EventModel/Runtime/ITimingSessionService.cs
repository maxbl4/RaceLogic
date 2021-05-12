using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface ITimingSessionService
    {
        TimingSession CreateSession(string name, Id<SessionDto> sessionId);
    }
}