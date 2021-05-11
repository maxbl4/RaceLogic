using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class RecordingSessionDto : IHasId<RecordingSessionDto>, IHasRunning, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<EventDto> EventId { get; set; }
        public Id<RecordingSessionDto> Id { get; set; }
        public string Name { get; set; }
        public string CheckpointServiceAddress { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    public class TimingSessionDto : IHasId<TimingSessionDto>, IHasName, IHasTimestamp, IHasRunning, IHasSeed, IHasPublished
    {
        public Id<SessionDto> SessionId { get; set; }
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public Id<EventDto> EventId { get; set; }
        public bool IsRunning { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public Id<TimingSessionDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}