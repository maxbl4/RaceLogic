using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace maxbl4.RaceLogic.LogManagement
{
    public class LogManager
    {
        private readonly string rootPath;
        private readonly NameProvider nameProvider = new NameProvider();

        public List<Event> Events { get; set; }

        public LogManager(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public void Load()
        {
            Events = new DirectoryInfo(rootPath)
                .GetDirectories()
                .Select(x => nameProvider.ParseName(x.Name))
                .Where(x => x.Success)
                .OrderBy(x => x.Name.Timestamp)
                .Select(x => LoadEvent(x.Name))
                .ToList();
        }

        Event LoadEvent(LogName name)
        {
            return new Event{
                Name = name,
                Sessions = new DirectoryInfo(Path.Combine(rootPath, name.Filename))
                    .GetFiles()
                    .Select(x => nameProvider.ParseName(x.Name))
                    .Where(x => x.Success)
                    .OrderBy(x => x.Name.Timestamp)
                    .Select(x => LoadSession(x.Name))
                    .ToList()
            };
        }

        Session LoadSession(LogName name)
        {
            return new Session{Name = name};
        }


    }

    public class Event
    {
        public LogName Name { get; set; }
        public List<Session> Sessions { get; set; }
    }

    public class Session
    {
        public LogName Name { get; set; }
    }

    public class NameProvider
    {
        public const string TimestampFormat = "yyyy-MM-dd_HH-mm-ssZ";
        private readonly string invalidCharsPattern;

        public NameProvider()
        {
            var invalidChars = new HashSet<char>();
            invalidChars.UnionWith(Path.GetInvalidPathChars());
            invalidChars.UnionWith(Path.GetInvalidFileNameChars());
            invalidChars.Add('+');
            invalidCharsPattern = string.Join("|", 
                invalidChars.OrderBy(x => x).Select(x => @"\u" + ((ushort)x).ToString("x4")));
        }

        public (bool Success, LogName Name) ParseName(string filename)
        {
            var match = Regex.Match(filename, @"(\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}Z)_(.*)");
            if (!match.Success) return (false, null);
            return (true, new LogName(
                DateTime.ParseExact(match.Groups[1].Value, TimestampFormat, 
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                PathDecode(match.Groups[2].Value), filename));
        }
        
        public string SerializeName(LogName name)
        {
            return name.Timestamp.ToString(TimestampFormat) + "_" + PathEncode(name.Name);
        }

        public string PathEncode(string original)
        {
            return Regex.Replace(original, invalidCharsPattern, 
                m => "+" + ((ushort)m.Value[0]).ToString("x4"));
        }

        public string PathDecode(string encoded)
        {
            return Regex.Replace(encoded, @"\+([a-f0-9]{4})",
                m => new string((char)ushort.Parse(m.Groups[1].Value, NumberStyles.HexNumber), 1));
        }
    }
    
    public class LogName
    {
        public DateTime Timestamp { get; }
        public string Name { get; }
        public string Filename { get; }

        public LogName(DateTime timestamp, string name, string filename)
        {
            Timestamp = timestamp;
            Name = name;
            Filename = filename;
        }
    }
}