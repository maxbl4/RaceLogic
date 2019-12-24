using System;

namespace maxbl4.Race.Logic.EventModel
{
    public interface ITimestamp
    {
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
    }
}