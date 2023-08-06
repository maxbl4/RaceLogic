using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface ITimingSessionService
    {
        void StartNewSession(string name, Id<SessionDto> sessionId);
        void Initialize();
        void StopSession();
    }
}