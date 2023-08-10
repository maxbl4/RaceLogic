using System;
using System.Xml;
using Newtonsoft.Json;

namespace maxbl4.Race.Logic.Extensions;

public class TimeSpanConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(XmlConvert.ToString((TimeSpan)value));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value is string s)
            return XmlConvert.ToTimeSpan(s);
        return TimeSpan.Zero;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TimeSpan);
    }
}