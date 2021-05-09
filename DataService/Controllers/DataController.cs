using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly UpstreamDataSyncService syncService;
        private readonly UpstreamDataRepository syncStorage;

        public DataController(UpstreamDataSyncService syncService, UpstreamDataRepository syncStorage)
        {
            this.syncService = syncService;
            this.syncStorage = syncStorage;
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