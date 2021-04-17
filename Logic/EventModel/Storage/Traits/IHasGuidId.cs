using maxbl4.Race.Logic.EventModel.Storage.Identifier;

namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public interface IHasGuidId
    {
        IGuidValue Id { get; set; }
    }
}