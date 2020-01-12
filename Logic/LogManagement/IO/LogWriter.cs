using System.IO;
using maxbl4.Race.Logic.LogManagement.EntryTypes;
using Newtonsoft.Json;

namespace maxbl4.Race.Logic.LogManagement.IO
{
    public class LogWriter
    {
        private readonly TextWriter textWriter;
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

        public void Append(IEntry entry)
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

        void AppendImpl(TextWriter tw, IEntry entry)
        {
            serializer.Serialize(tw, entry);
            tw.WriteLine();
            tw.Flush();
        }
    }
}