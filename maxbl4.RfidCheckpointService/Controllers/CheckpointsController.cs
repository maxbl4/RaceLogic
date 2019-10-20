using System;
using System.Collections.Generic;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("cp")]
    public class CheckpointsController : ControllerBase
    {
        private readonly StorageService storageService;

        public CheckpointsController(StorageService storageService)
        {
            this.storageService = storageService;
        }

        [HttpGet]
        public IEnumerable<Checkpoint> Get(DateTime? start = null, DateTime? end = null)
        {
            return storageService.ListCheckpoints(start, end);
        }
    }
}