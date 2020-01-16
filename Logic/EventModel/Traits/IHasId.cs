using System;

namespace maxbl4.Race.Logic.EventModel.Traits
{
    public interface IHasId<T> : IHasTraits
    {
        Id<T> Id { get; set; }
    }

    public interface IHasName : IHasTraits
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}