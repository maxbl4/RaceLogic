using System;

namespace maxbl4.Race.Logic.EventModel.Traits
{
    public interface IHasIdentifiers
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
    }
}