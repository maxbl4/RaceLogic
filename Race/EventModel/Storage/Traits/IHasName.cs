namespace maxbl4.Race.EventModel.Storage.Traits
{
    public interface IHasName : IHasTraits
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}