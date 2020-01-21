using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSession: IHasName, IHasTimestamp
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
     
        public Id<SessionDto> SessionDtoId { get; set; }
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public List<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();
        public IFinishCriteria FinishCriteria { get; set; }
        public ITrackOfCheckpoints Track { get; private set; }

        public void Initialize()
        {
            Track = new TrackOfCheckpoints(StartTime, FinishCriteria);
        }

        public void AppendCheckpoint(Checkpoint checkpoint)
        {
        }
    }
}