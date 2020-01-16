using System;

namespace maxbl4.Race.Logic.EventModel.Traits
{
    public static class TraitsExt
    {
        public static T ApplyTraits<T>(this T obj)
            where T: IHasTraits
        {
            if (obj is IHasId<T> hasIdentifiers)
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

            return obj;
        }
    }
}