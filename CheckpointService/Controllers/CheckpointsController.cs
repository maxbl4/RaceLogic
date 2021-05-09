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
        private readonly IRfidService rfidService;
        private readonly CheckpointRepository checkpointRepository;

        public CheckpointsController(CheckpointRepository checkpointRepository, IRfidService rfidService)
        {
            this.checkpointRepository = checkpointRepository;
            this.rfidService = rfidService;
        }

        [HttpGet]
        public IEnumerable<Checkpoint> Get(DateTime? start = null, DateTime? end = null)
        {
            return checkpointRepository.ListCheckpoints(start, end);
        }

        [HttpPut]
        [HttpPost]
        public IActionResult Put([FromBody] string riderId)
        {
            if (string.IsNullOrWhiteSpace(riderId))
                return NoContent();
            rfidService.AppendRiderId(riderId);
            return Accepted();
        }

        [HttpDelete("{id}")]
        public int Delete(Guid id)
        {
            return checkpointRepository.DeleteCheckpoint(id);
        }

        [HttpDelete]
        public int Delete(DateTime? start, DateTime? end)
        {
            return checkpointRepository.DeleteCheckpoints(start, end);
        }
    }
}