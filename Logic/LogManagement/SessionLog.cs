using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.LogManagement.EntryTypes;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.LogManagement
{
    public class SessionLog
    {
        private readonly IAutoMapperProvider autoMapperProvider;
        public List<object> LogEntries { get; } = new List<object>();
        public ITrackOfCheckpoints TrackOfCheckpoints { get; private set; } = new TrackOfCheckpoints();
        public IFinishCriteria FinishCriteria { get; private set; } = null;
        public DateTime StartTime = Constants.DefaultUtcDate;

        public SessionLog(IAutoMapperProvider autoMapperProvider)
        {
            this.autoMapperProvider = autoMapperProvider;
        }

        public void BatchLoad(IEnumerable<object> logEntries)
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

        public void Checkpoint(CheckpointDto checkpoint)
        {
            LogEntries.Add(checkpoint);
            //TrackOfCheckpoints.Append(checkpoint);
        }
        
        public void InsertCheckpoint(InsertCheckpointDto insert)
        {
            LogEntries.Add(insert);
            TrackOfCheckpoints = ReloadTrack(StartTime, FinishCriteria);
        }
        
        public void DropCheckpoint(DropCheckpointDto drop)
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
            // foreach (var checkpoint in FlattenCheckpointLog(LogEntries).Select(x => autoMapperProvider.Mapper.Map<Checkpoint>(x)))
            // {
            //     track.Append(checkpoint);
            // }
            return track;
        }

        public static IEnumerable<CheckpointDto> FlattenCheckpointLog<T>(IEnumerable<T> logEntries)
            where T: IHasId<T>
        {
            var drops = new HashSet<Id<CheckpointDto>>(logEntries.OfType<DropCheckpointDto>().Select(x => x.TargetId));
            var inserts = logEntries.OfType<InsertCheckpointDto>().Where(x => !drops.Contains(x.Id))
                .OrderBy(x => x.Timestamp)
                .ToList();
            foreach (var checkpoint in logEntries.OfType<CheckpointDto>().Where(x => !drops.Contains(x.Id)))
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