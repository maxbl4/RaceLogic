using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;

namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public static class TraitsExt
    {
        public static T ApplyTraits<T>(this T obj)
            where T : IHasTraits
        {
            if (obj is IHasId<T> hasIdentifiers)
                if (hasIdentifiers.Id == Id<T>.Empty)
                    hasIdentifiers.Id = Id<T>.NewId();

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
            var types = GetIHasIdTypes(assemblyToScanForTypes);
            foreach (var type in types)
            {
                mapper.RegisterType(type, obj => ((IGuidValue) obj).Value.ToString("N"),
                    value => Activator.CreateInstance(type, new Guid(value.AsString)));
            }
        }
        
        public static IEnumerable<Type> GetIHasIdTypes(Assembly assemblyToScanForTypes = null)
        {
            var idType = typeof(Id<>);
            if (assemblyToScanForTypes == null)
                assemblyToScanForTypes = Assembly.GetExecutingAssembly();
            var iHasTraits = typeof(IHasTraits);
            return assemblyToScanForTypes.GetTypes()
                .Select(x => (type: x, interfaces: x.GetInterfaces()))
                .Where(x => x.interfaces.Contains(iHasTraits))
                .Where(HasId)
                .Select(x => idType.MakeGenericType(x.type));
        }

        private static bool HasId((Type type, Type[] interfaces) def)
        {
            var hasIdType = typeof(IHasId<>).MakeGenericType(def.type);
            return def.interfaces.Contains(hasIdType);
        }
    }
}