namespace maxbl4.Race.Logic.EventModel.Traits
{
    public interface IHasPersonName: IHasTraits
    {
        string FirstName { get; set; }
        string ParentName { get; set; }
        string LastName { get; set; }
    }
}