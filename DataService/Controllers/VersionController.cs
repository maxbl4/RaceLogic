﻿using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("version")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        [Produces("text/plain")]
        public string Get()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}