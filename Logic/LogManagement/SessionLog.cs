using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.LogManagement.EntryTypes;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.LogManagement
{
    public class SessionLog
    {
        public List<IEntry> LogEntries { get; } = new List<IEntry>();
        public ITrackOfCheckpoints TrackOfCheckpoints { get; private set; } = new TrackOfCheckpoints();
        public IFinishCriteria FinishCriteria { get; private set; } = null;
        public DateTime StartTime = Constants.DefaultUtcDate;
        
        public void Start(SessionStart start)
        {
            LogEntries.Add(start);
            StartTime = start.Timestamp;
            FinishCriteria = new FinishCriteria(start.Duration, start.TotalLaps, start.LapsAfterDuration,
                start.SkipStartingCheckpoint, start.ForceFinishOnly);
            TrackOfCheckpoints = ReloadTrack(StartTime, FinishCriteria);
        }

        public void Checkpoint(Checkpoint checkpoint)
        {
            LogEntries.Add(checkpoint);
            TrackOfCheckpoints.Append(checkpoint);
        }
        
        public void DropCheckpoint(DropCheckpoint drop)
        {
            LogEntries.Add(drop);
            TrackOfCheckpoints = ReloadTrack(StartTime, FinishCriteria);
        }
        
        public void Comment(Comment comment)
        {
            LogEntries.Add(comment);
        }

        ITrackOfCheckpoints ReloadTrack(DateTime startTime, IFinishCriteria finishCriteria)
        {
            var track = new TrackOfCheckpoints(startTime, finishCriteria);
            foreach (var checkpoint in FlattenCheckpointLog())
            {
                track.Append(checkpoint);
            }
            return track;
        }

        IEnumerable<Checkpoint> FlattenCheckpointLog()
        {
            var drops = new HashSet<long>(LogEntries.OfType<DropCheckpoint>().Select(x => x.Id));
            //TODO: Apply dropped/altered checkpoints before returning
            return LogEntries.OfType<Checkpoint>()
                .Where(x => !drops.Contains(x.Id));
        }
    }
}