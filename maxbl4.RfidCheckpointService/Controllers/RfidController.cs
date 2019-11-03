using maxbl4.RfidCheckpointService.Rfid;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.RfidCheckpointService.Controllers
{
    [ApiController]
    [Route("rfid")]
    public class RfidController : ControllerBase
    {
        private readonly RfidService rfidService;
        private readonly StorageService storageService;

        public RfidController(RfidService rfidService, StorageService storageService)
        {
            this.rfidService = rfidService;
            this.storageService = storageService;
        }

        [HttpGet]
        public RfidOptions Get()
        {
            return storageService.GetRfidSettings();
        }
    }
}