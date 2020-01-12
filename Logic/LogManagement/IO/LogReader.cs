using System.Collections.Generic;
using System.IO;
using maxbl4.Race.Logic.LogManagement.EntryTypes;
using Newtonsoft.Json;

namespace maxbl4.Race.Logic.LogManagement.IO
{
    public class LogReader
    {
        private readonly JsonSerializer serializer = new SerializerFactory().Create();

        public IEnumerable<IEntry> Read(string filename)
        {
            using (var sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                return Read(sr);
        }
        
        public IEnumerable<IEntry> Read(TextReader tr)
        {
            string s;
            while ((s = tr.ReadLine()) != null)
            {
                var o = serializer.Deserialize(new JsonTextReader(new StringReader(s)));
                if (o is IEntry entry)
                    yield return entry;
            }
        }
    }
}