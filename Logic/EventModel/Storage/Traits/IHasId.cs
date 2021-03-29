using System;

namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public interface IHasId<T> : IHasTraits
    {
        Guid Id { get; set; }
    }
}