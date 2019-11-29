﻿using System;
using System.Collections.Generic;
using maxbl4.RfidCheckpointService.Model;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("tag")]
    public class TagController : ControllerBase
    {
        private readonly StorageService storageService;

        public TagController(StorageService storageService)
        {
            this.storageService = storageService;
        }

        [HttpGet]
        public IEnumerable<Tag> Get(DateTime? start = null, DateTime? end = null, int? count = null)
        {
            if (count == null)
                count = 100;
            return storageService.ListTags(start, end, count);
        }
        
        [HttpDelete]
        public int Delete(DateTime? start, DateTime? end)
        {
            return storageService.DeleteTags(start, end);
        }
    }
}