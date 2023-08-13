using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;

namespace maxbl4.Race.Logic.WebModel;

public class RiderEventInfoUpdate
{
    public List<RiderEventInfoDto> Riders { get; set; }
    public Id<TimingSessionDto> TimingSessionId { get; set; }
}