using System;

namespace maxbl4.Race.Logic.EventModel.Traits
{
    public interface IHasTimestamp : IHasTraits
    {
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
    }
}