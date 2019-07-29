using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using maxbl4.RaceLogic.LogManagement.EntryTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace maxbl4.RaceLogic.LogManagement
{
    public class LogWriter
    {
        private readonly TextWriter textWriter = null;
        private readonly string filename;
        private readonly JsonSerializer serializer = new SerializerFactory().Create();

        public LogWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }
        
        public LogWriter(string filename)
        {
            this.filename = filename;
        }

        public void Append(Entry entry)
        {
            if (textWriter != null)
                AppendImpl(textWriter, entry);
            else
            {
                using (var tw = new StreamWriter(filename, true))
                {
                    AppendImpl(tw, entry);
                }
            }
        }

        void AppendImpl(TextWriter tw, Entry entry)
        {
            serializer.Serialize(tw, entry);
            tw.WriteLine();
            tw.Flush();
        }
    }
    
    public class LogReader
    {
        private readonly JsonSerializer serializer = new SerializerFactory().Create();

        public List<Entry> ReadAll(string filename)
        {
            using (var sr = new StreamReader(filename))
                return ReadAll(sr);
        }
        
        public List<Entry> ReadAll(TextReader tr)
        {
            var result = new List<Entry>();
            string s;
            while ((s = tr.ReadLine()) != null)
            {
                var o = serializer.Deserialize(new JsonTextReader(new StringReader(s)));
                if (o is Entry entry)
                    result.Add(entry);
            }
            return result;
        }
    }
}