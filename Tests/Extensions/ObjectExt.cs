using LiteDB;

namespace maxbl4.Race.Tests.Extensions
{
    public static class ObjectExt
    {
        public static BsonValue ToBson(this object obj)
        {
            return obj == null ? null : BsonMapper.Global.Serialize(obj);
        }

        public static string ToLiteDbJson(this object obj)
        {
            return obj == null ? null : JsonSerializer.Serialize(obj.ToBson());
        }
    }
}