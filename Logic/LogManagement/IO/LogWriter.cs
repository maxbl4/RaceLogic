using System.IO;
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

        public void Append<T>(T entry)
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

        void AppendImpl<T>(TextWriter tw, T entry)
        {
            serializer.Serialize(tw, entry);
            tw.WriteLine();
            tw.Flush();
        }
    }
}