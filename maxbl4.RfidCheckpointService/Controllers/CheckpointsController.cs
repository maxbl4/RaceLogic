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
        public void Put([FromBody]string riderId)
        {
            rfidService.AppendRiderId(riderId);
        }
        
        [HttpDelete("{id}")]
        public int Delete(long id)
        {
            return storageService.DeleteCheckpoint(id);
        }
        
        [HttpDelete()]
        public int Delete(DateTime? start, DateTime? end)
        {
            return storageService.DeleteCheckpoints(start, end);
        }
    }
}