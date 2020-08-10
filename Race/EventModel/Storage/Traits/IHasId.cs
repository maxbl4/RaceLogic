using maxbl4.Race.EventModel.Storage.Identifier;

namespace maxbl4.Race.EventModel.Storage.Traits
{
    public interface IHasId<T> : IHasTraits
    {
        Id<T> Id { get; set; }
    }
}