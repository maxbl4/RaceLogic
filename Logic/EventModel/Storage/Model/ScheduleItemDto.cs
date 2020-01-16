using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class ScheduleItemDto: IHasId<ScheduleItemDto>, IHasName, IHasTimestamp, IHasPublished, IHasSeed
    {
        public Id<ScheduleItemDto> Id { get; set; }
        public string Name {get;set;}
        public string Description {get;set;}
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
        public DateTime StartTime {get;set;}
        public TimeSpan Duration {get;set;}
        public Id<EventDto> EventId {get;set;}
        public Id<SessionDto> SessionId { get; set; }
    }
}