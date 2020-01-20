using System;
using System.Linq;
using System.Reflection;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;

namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public static class TraitsExt
    {
        public static T ApplyTraits<T>(this T obj)
            where T: IHasTraits
        {
            if (obj is IHasId<T> hasIdentifiers)
            {
                if (hasIdentifiers.Id == Id<T>.Empty)
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

        public static void RegisterIdBsonMappers(this BsonMapper mapper, Assembly assemblyToScanForTypes = null)
        {
            if (assemblyToScanForTypes == null)
                assemblyToScanForTypes = Assembly.GetExecutingAssembly();
            var iHasTraits = typeof(IHasTraits);
            var types = assemblyToScanForTypes.GetTypes()
                .Select(x => (type: x, interfaces: x.GetInterfaces()))
                .Where(x => x.interfaces.Contains(iHasTraits))
                .Where(HasId);
            var idType = typeof(Id<>);
            foreach (var type in types)
            {
                var idClosed = idType.MakeGenericType(type.type);
                mapper.RegisterType(idClosed, obj => ((IGuidValue)obj).Value.ToString("N"), value => Activator.CreateInstance(idClosed, new Guid(value.AsString)));
            }
        }

        static bool HasId((Type type, Type[] interfaces) def)
        {
            var hasIdType = typeof(IHasId<>).MakeGenericType(def.type);
            return def.interfaces.Contains(hasIdType);
        }
    }
}