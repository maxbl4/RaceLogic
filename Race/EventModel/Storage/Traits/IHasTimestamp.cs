using System;

namespace maxbl4.Race.EventModel.Storage.Traits
{
    public interface IHasTimestamp : IHasTraits
    {
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
    }
}