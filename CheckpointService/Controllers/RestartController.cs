using System;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.CheckpointService.Controllers
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