using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace maxbl4.Race.Tests.Extensions
{
    public static class HttpClientExt
    {
        public static Task<T> GetBsonAsync<T>(this HttpClient http, Uri uri)
        {
            return GetBsonAsync<T>(http, uri.ToString());
        }
        
        public static async Task<T> GetBsonAsync<T>(this HttpClient http, string uri)
        {
            if (typeof(T).IsSubclassOf(typeof(BsonValue)))
                return (T)(object)JsonSerializer.Deserialize(await http.GetStringAsync(uri));
            return BsonMapper.Global.Deserialize<T>(JsonSerializer.Deserialize(await http.GetStringAsync(uri)));
        }

        public static async Task<HttpResponseMessage> PostBsonAsync(this HttpClient http, Uri uri, object value)
        {
            return await http.PostBsonAsync(uri.ToString(), value);
        }
        
        public static async Task<HttpResponseMessage> PostBsonAsync(this HttpClient http, string uri, object value)
        {
            return await http.PostAsync(uri,
                new StringContent(value.ToLiteDbJson(), Encoding.UTF8, "application/json"));
        }
    }
}