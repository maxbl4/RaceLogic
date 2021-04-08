using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class DeviceDescriptorDto : IHasId<DeviceDescriptorDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<DeviceDescriptorDto> Id { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public DeviceType Type { get; set; }
    }

    public enum DeviceType
    {
        Unknown,
        CheckpointService,
        LedDisplay
    }

    public class RecordingSessionDto : IHasId<RecordingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<SessionDto> SessionId { get; set; }
        public Id<RecordingSessionDto> Id { get; set; }
        public string Name { get; set; }
        public string CheckpointServiceAddress { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

    public class RatingSessionDto : IHasId<RatingSessionDto>, IHasSeed, IHasTimestamp, IHasName
    {
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public Id<FinishCriteriaDto> FinishCriteriaId { get; set; }
        public Id<RatingSessionDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}