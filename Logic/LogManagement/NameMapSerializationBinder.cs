using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.LogManagement.EntryTypes;
using Newtonsoft.Json.Serialization;

namespace maxbl4.Race.Logic.LogManagement
{
    public class NameMapSerializationBinder: ISerializationBinder
    {
        private readonly Dictionary<string, Type> nameToTypeMap = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> typeToNameMap = new Dictionary<Type, string>();
        public NameMapSerializationBinder(IEnumerable<KeyValuePair<string, Type>> nameToTypeMap)
        {
            foreach (var pair in nameToTypeMap)
            {
                this.nameToTypeMap[pair.Key] = pair.Value;
                typeToNameMap[pair.Value] = pair.Key;
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (nameToTypeMap.TryGetValue(typeName, out var t)) return t;
            throw new NotSupportedException($"No mapping defined for typeName {typeName}");
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (serializedType.IsGenericType)
            {
                var gen = serializedType.GetGenericTypeDefinition();
                if (gen == typeof(Id<>))
                {
                    typeName = "id";
                    return;
                }
            }
            if (typeToNameMap.TryGetValue(serializedType, out typeName)) return;
            throw new NotSupportedException($"No mapping defined for type {serializedType}");
        }

        public static NameMapSerializationBinder CreateDefault()
        {
            return new NameMapSerializationBinder(new Dictionary<string, Type>{
                {"cp", typeof(Checkpoint)},
                {"drop", typeof(DropCheckpoint)},
                {"start", typeof(SessionStart)},
                {"comment", typeof(Comment)}
            });
        }
    }
}