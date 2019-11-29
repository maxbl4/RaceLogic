using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using maxbl4.Infrastructure;
using maxbl4.RfidCheckpointService.Model;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("log")]
    public class LogController : ControllerBase
    {
        private const string dataDirectory = "var/data";
        
        private readonly RollingFileInfo mainLogFile = new RollingFileInfo(Path.Combine(dataDirectory, "RfidCheckpointServiceRunner.log"));
        private readonly RollingFileInfo errorLogFile = new RollingFileInfo(Path.Combine(dataDirectory, "RfidCheckpointServiceRunner-errors.log"));
        
        [HttpGet]
        public IActionResult Get(int? lines, string filter, bool? errors)
        {
            var logName = errors == true ? errorLogFile.CurrentFile : mainLogFile.CurrentFile;

            if (logName == null)
                NotFound();
            if (lines == null)
                lines = 50;
            if (lines < 0)
                lines = Int32.MaxValue;
            var logText = new StreamReader(new FileStream(logName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).ReadToEnd();
            return Content(string.Join("\r\n", logText.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => string.IsNullOrEmpty(filter) || x.Contains(filter))
                .TakeLast(lines.Value)), "text/plain");
        }

        [HttpGet("info")]
        public IEnumerable<LogFileInfo> Info()
        {
            return errorLogFile.AllCurrentFiles.Concat(mainLogFile.AllCurrentFiles)
                .Select(x => new FileInfo(x))
                .Select(x => new LogFileInfo {File = x.Name, Size = (int) x.Length});
        }

        [HttpDelete]
        public int Delete(bool? errors)
        {
            if (errors == true)
                return errorLogFile.Delete();
            return mainLogFile.Delete();
        }
    }
}