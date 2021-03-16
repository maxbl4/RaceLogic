using System;
using Newtonsoft.Json;

namespace maxbl4.Race.Logic.EventModel.Storage.Identifier
{
    public class IdJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var iGuidValue = (IGuidValue) value;
            writer.WriteValue(iGuidValue.Value.ToString("N"));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (Guid.TryParse(reader.Value as string, out var g))
                return Activator.CreateInstance(objectType, g);
            return Activator.CreateInstance(objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Id<>);
        }
    }
}