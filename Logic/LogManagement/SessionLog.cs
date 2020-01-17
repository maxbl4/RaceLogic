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

        public void BatchLoad(IEnumerable<IEntry> logEntries)
        {
            LogEntries.Clear();
            LogEntries.AddRange(logEntries);
            var start = LogEntries.OfType<SessionStart>().LastOrDefault();
            if (start != null)
                ApplyStart(start);
            else
                TrackOfCheckpoints = ReloadTrack(StartTime, FinishCriteria);
        }
        
        public void Start(SessionStart start)
        {
            LogEntries.Add(start);
            ApplyStart(start);
        }

        public void Checkpoint(Checkpoint checkpoint)
        {
            //LogEntries.Add(checkpoint);
            TrackOfCheckpoints.Append(checkpoint);
        }
        
        public void InsertCheckpoint(InsertCheckpoint insert)
        {
            //LogEntries.Add(insert);
            TrackOfCheckpoints = ReloadTrack(StartTime, FinishCriteria);
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
        
        void ApplyStart(SessionStart start)
        {
            StartTime = start.Timestamp;
            FinishCriteria = new FinishCriteria(start.Duration, start.TotalLaps, start.LapsAfterDuration,
                start.SkipStartingCheckpoint, start.ForceFinishOnly);
            TrackOfCheckpoints = ReloadTrack(StartTime, FinishCriteria);
        }

        ITrackOfCheckpoints ReloadTrack(DateTime startTime, IFinishCriteria finishCriteria)
        {
            var track = new TrackOfCheckpoints(startTime, finishCriteria);
            foreach (var checkpoint in FlattenCheckpointLog(LogEntries))
            {
                track.Append(checkpoint);
            }
            return track;
        }

        public static IEnumerable<Checkpoint> FlattenCheckpointLog(IEnumerable<IEntry> logEntries)
        {
            var drops = new HashSet<long>(logEntries.OfType<DropCheckpoint>().Select(x => x.Id));
            var inserts = logEntries.OfType<InsertCheckpoint>().Where(x => !drops.Contains(x.Id))
                .OrderBy(x => x.Timestamp)
                .ToList();
            foreach (var checkpoint in logEntries.OfType<Checkpoint>().Where(x => !drops.Contains(x.Id)))
            {
                while (inserts.Count > 0 && inserts[0].Timestamp < checkpoint.Timestamp)
                {
                    yield return inserts[0];
                    inserts.RemoveAt(0);
                }
                yield return checkpoint;
            }

            foreach (var insert in inserts)
            {
                yield return insert;
            }
        }
    }
}