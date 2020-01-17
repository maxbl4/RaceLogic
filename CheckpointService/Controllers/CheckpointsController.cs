using System;
using System.Collections.Generic;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.Logic.Checkpoints;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.CheckpointService.Controllers
{
    [ApiController]
    [Route("cp")]
    public class CheckpointsController : ControllerBase
    {
        private readonly StorageService storageService;
        private readonly IRfidService rfidService;

        public CheckpointsController(StorageService storageService, IRfidService rfidService)
        {
            this.storageService = storageService;
            this.rfidService = rfidService;
        }

        [HttpGet]
        public IEnumerable<Checkpoint> Get(DateTime? start = null, DateTime? end = null)
        {
            return storageService.ListCheckpoints(start, end);
        }
        
        [HttpPut]
        [HttpPost]
        public IActionResult Put([FromBody]string riderId)
        {
            if (string.IsNullOrWhiteSpace(riderId))
                return NoContent();
            rfidService.AppendRiderId(riderId);
            return Accepted();
        }
        
        [HttpDelete("{id}")]
        public int Delete(Guid id)
        {
            return storageService.DeleteCheckpoint(id);
        }
        
        [HttpDelete]
        public int Delete(DateTime? start, DateTime? end)
        {
            return storageService.DeleteCheckpoints(start, end);
        }
    }
}