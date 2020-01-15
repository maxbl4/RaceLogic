using System;

namespace maxbl4.Race.Logic.EventModel.Traits
{
    public interface IHasTimestamp
    {
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
    }
}