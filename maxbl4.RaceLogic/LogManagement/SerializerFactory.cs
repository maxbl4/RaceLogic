using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace maxbl4.RaceLogic.LogManagement
{
    public class SerializerFactory
    {
        public JsonSerializer Create()
        {
            return new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                SerializationBinder = NameMapSerializationBinder.CreateDefault(),
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new WritablePropertiesOnlyResolver(),
                CheckAdditionalContent = true
            };
        }
        
        class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }
    }
}