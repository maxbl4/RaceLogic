using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSessionStaticData
    {
        public SessionDto SessionDefinition { get; set; }
        public IFinishCriteria FinishCriteria { get; set; }
        public TimeSpan MinLap { get; set; }
        public ConcurrentDictionary<string, List<Id<RiderClassRegistrationDto>>> RiderIdMap { get; set; }
    }
}