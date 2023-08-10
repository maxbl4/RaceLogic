using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime;

public record RatingUpdatedMessage(List<RoundPosition> Rating, Id<TimingSessionDto> TimingSessionId);