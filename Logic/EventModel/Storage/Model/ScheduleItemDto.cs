using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class ScheduleItemDto : IHasId<ScheduleItemDto>, IHasName, IHasTimestamp, IHasPublished, IHasSeed
    {
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Guid EventId { get; set; }
        public Guid SessionId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}