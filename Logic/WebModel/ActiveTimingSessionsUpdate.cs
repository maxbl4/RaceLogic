using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Model;

namespace maxbl4.Race.Logic.WebModel;

public class ActiveTimingSessionsUpdate
{
    public List<TimingSessionDto> Sessions { get; set; }
}