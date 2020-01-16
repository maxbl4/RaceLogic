namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public interface IHasPersonName: IHasTraits
    {
        string FirstName { get; set; }
        string ParentName { get; set; }
        string LastName { get; set; }
    }
}