using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.UpstreamData;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController: ControllerBase
    {
        private readonly StorageService storageService;
        private readonly UpstreamDataSyncService syncService;
        private readonly UpstreamDataStorageService syncStorage;

        public DataController(StorageService storageService, UpstreamDataSyncService syncService, UpstreamDataStorageService syncStorage)
        {
            this.storageService = storageService;
            this.syncService = syncService;
            this.syncStorage = syncStorage;
        }

        [HttpGet("event/{id}")]
        public ActionResult<EventDto> GetEvent(Guid id)
        {
            return storageService.GetEvent(id);
        }
        
        [HttpGet("event")]
        public ActionResult<List<EventDto>> ListEvents()
        {
            return storageService.ListEvents();
        }
        
        //[HttpPost("event/{id?}")]
        [HttpPut("event/{id?}")]
        public Guid UpsertEvent(Id<EventDto> id, EventDto entity)
        {
            if (id != Id<EventDto>.Empty)
                entity.Id = id;
            storageService.UpsertEvent(entity);
            return entity.Id;
        }
        
        [HttpDelete("event/{id}")]
        public void DeleteEvent(Id<EventDto> id)
        {
            storageService.DeleteEvent(id);
        }
        
        [HttpPost("upstream/purge")]
        public ActionResult PurgeUpstreamData()
        {
            syncStorage.PurgeExistingData();
            return Ok();
        }

        [HttpPost("upstream")]
        public async Task<bool> DownloadUpstreamData(bool forceFullSync = false)
        {
            return await syncService.Download(forceFullSync);
        }
        
        [HttpGet("series")]
        public ActionResult<List<SeriesDto>> ListSeries()
        {
            return syncStorage.ListSeries().ToList();
        }
        
        [HttpGet("championships")]
        public ActionResult<List<ChampionshipDto>> ListChampionships([FromQuery]Id<SeriesDto> value)
        {
            return syncStorage.ListChampionships(value).ToList();
        }
        
        [HttpGet("classes")]
        public ActionResult<List<ClassDto>> ListClasses([FromQuery]Id<ChampionshipDto> value)
        {
            return syncStorage.ListClasses(value).ToList();
        }
        
        [HttpGet("events")]
        public ActionResult<List<EventDto>> ListEvents([FromQuery]Id<ChampionshipDto> value)
        {
            return syncStorage.ListEvents(value).ToList();
        }
    }
}