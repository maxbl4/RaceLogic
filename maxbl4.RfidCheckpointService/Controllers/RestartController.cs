using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("restart")]
    public class RestartController: ControllerBase
    {
        [HttpPost]
        public IActionResult Post()
        {
            Environment.Exit(0);
            return Ok();
        }
    }
}