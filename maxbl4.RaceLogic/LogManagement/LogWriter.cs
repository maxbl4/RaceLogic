using System.IO;
using Newtonsoft.Json;

namespace maxbl4.RaceLogic.LogManagement
{
    public class LogWriter
    {
        private readonly string filename;
        private readonly JsonSerializer serializer;

        public LogWriter(string filename)
        {
            this.filename = filename;
            serializer = SetupSerializer();
        }

        public void Append(object entry)
        {
            using (var sw = new StreamWriter(filename))
            {
                serializer.Serialize(sw, entry);
                sw.WriteLine();
            }
        }

        private JsonSerializer SetupSerializer()
        {
            return new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                SerializationBinder = NameMapSerializationBinder.CreateDefault(),
                TypeNameHandling = TypeNameHandling.All
            };
        }
    }
}