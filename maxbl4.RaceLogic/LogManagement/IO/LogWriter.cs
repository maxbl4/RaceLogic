using System.IO;
using maxbl4.RaceLogic.LogManagement.EntryTypes;
using Newtonsoft.Json;

namespace maxbl4.RaceLogic.LogManagement.IO
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
}