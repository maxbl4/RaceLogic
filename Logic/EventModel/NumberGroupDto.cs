using System;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Logic.EventModel
{
    public class NumberGroupDto: IHasId<NumberGroupDto>, IHasName, IHasTimestamp, IHasSeed
    {
        public Id<NumberGroupDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsSeed { get; set; }
    }
}