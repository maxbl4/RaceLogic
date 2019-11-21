using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("log")]
    public class LogController : ControllerBase
    {
        [HttpGet("{lineCount?}/{filter?}")]
        public string Get(int? lineCount, string filter)
        {
            const string logName = "RfidCheckpointServiceRunner.log";
            
            if (!System.IO.File.Exists(logName))
                return $"Log file not found {logName}";
            if (lineCount == null)
                lineCount = 50;
            if (lineCount < 0)
                lineCount = Int32.MaxValue;
            var logText = new StreamReader(new FileStream(logName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).ReadToEnd();
            return string.Join("\r\n", logText.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => string.IsNullOrEmpty(filter) || x.Contains(filter))
                .TakeLast(lineCount.Value));
        }
    }
}