using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using maxbl4.RfidCheckpointService.Model;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("log")]
    public class LogController : ControllerBase
    {
        private const string logFilenamePattern = "RfidCheckpointServiceRunner*.log";
        
        [HttpGet()]
        public string Get(int? lines, string filter)
        {
            var logName = Directory.GetFiles(Environment.CurrentDirectory, logFilenamePattern).OrderByDescending(x => x).FirstOrDefault();
            
            if (logName == null)
                return $"Log file not found {logName}";
            if (lines == null)
                lines = 50;
            if (lines < 0)
                lines = Int32.MaxValue;
            var logText = new StreamReader(new FileStream(logName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).ReadToEnd();
            return string.Join("\r\n", logText.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => string.IsNullOrEmpty(filter) || x.Contains(filter))
                .TakeLast(lines.Value));
        }

        [HttpGet("info")]
        public IEnumerable<LogFileInfo> Info()
        {
            return new DirectoryInfo(Environment.CurrentDirectory)
                .GetFiles(logFilenamePattern)
                .OrderBy(x => x.Name)
                .Select(x => new LogFileInfo {File = x.Name, Size = (int) x.Length});
        }

        [HttpDelete]
        public int Delete()
        {
            var deleted = 0;
            foreach (var f in Directory.GetFiles(Environment.CurrentDirectory, logFilenamePattern))
            {
                try
                {
                    System.IO.File.Delete(f);
                    deleted++;
                }
                catch {}
            }

            return deleted;
        }
    }
}