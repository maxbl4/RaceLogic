using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface IRecordingService
    {
        void StopRecording();
        void StartRecording(Id<EventDto> eventId);
        IDisposable Subscribe(IObserver<Checkpoint> observer, DateTime from);
        void AppendCheckpoint(Id<RecordingSessionDto> sessionId, Checkpoint checkpoint);
    }
}