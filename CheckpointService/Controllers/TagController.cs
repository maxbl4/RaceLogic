using System;
using System.Collections.Generic;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.CheckpointService.Model;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.CheckpointService.Controllers
{
    [ApiController]
    [Route("tag")]
    public class TagController : ControllerBase
    {
        private readonly CheckpointRepository checkpointRepository;

        public TagController(CheckpointRepository checkpointRepository)
        {
            this.checkpointRepository = checkpointRepository;
        }

        [HttpGet]
        public IEnumerable<Tag> Get(DateTime? start = null, DateTime? end = null, int? count = null)
        {
            if (count == null)
                count = 100;
            return checkpointRepository.ListTags(start, end, count);
        }

        [HttpDelete]
        public int Delete(DateTime? start, DateTime? end)
        {
            return checkpointRepository.DeleteTags(start, end);
        }
    }
}