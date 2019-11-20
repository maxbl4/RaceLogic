using System;
using System.Linq;
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
            return storageService.GetRfidOptions();
        }
        
        [HttpGet("{property}")]
        public object Get(string property)
        {
            var opts = storageService.GetRfidOptions();
            var prop = opts.GetType().GetProperties()
                .FirstOrDefault(x => x.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
                return NotFound();
            var value = prop.GetValue(opts);
            if (value is string)
                return $"\"{value}\"";
            return value;
        }
        
        [HttpPost("{property}")]
        [HttpPut("{property}")]
        public object Put(string property, [FromBody]object newValue)
        {
            var opts = storageService.GetRfidOptions();
            var prop = opts.GetType().GetProperties()
                .FirstOrDefault(x => x.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
                return NotFound();
            prop.SetValue(opts, Convert.ChangeType(newValue, prop.PropertyType));
            storageService.SetRfidOptions(opts);
            return Ok();
        }
        
        [HttpPost]
        [HttpPut]
        public void Put([FromBody]RfidOptions options)
        {
            storageService.SetRfidOptions(options);
        }
        
        [HttpDelete]
        public void Delete()
        {
            storageService.SetRfidOptions(RfidOptions.Default);
        }
    }
}