using System.Collections.Generic;
using System.IO;
using maxbl4.RaceLogic.LogManagement.EntryTypes;
using Newtonsoft.Json;

namespace maxbl4.RaceLogic.LogManagement.IO
{
    public class LogReader
    {
        private readonly JsonSerializer serializer = new SerializerFactory().Create();

        public IEnumerable<Entry> Read(string filename)
        {
            using (var sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                return Read(sr);
        }
        
        public IEnumerable<Entry> Read(TextReader tr)
        {
            string s;
            while ((s = tr.ReadLine()) != null)
            {
                var o = serializer.Deserialize(new JsonTextReader(new StringReader(s)));
                if (o is Entry entry)
                    yield return entry;
            }
        }
    }
}