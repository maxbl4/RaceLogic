using System;

namespace maxbl4.Race.Logic.EventModel.Traits
{
    public class Traits
    {
        public static void Apply<T>(T obj)
        {
            if (obj is IHasIdentifiers<T> hasIdentifiers)
            {
                if (hasIdentifiers.Id.IsEmpty)
                    hasIdentifiers.Id = Id<T>.NewId();
            }

            if (obj is IHasTimestamp timestamp)
            {
                if (timestamp.Created == default)
                    timestamp.Created = DateTime.UtcNow;
                timestamp.Updated = DateTime.UtcNow;
            }
        }
    }
}