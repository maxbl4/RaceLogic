using System;

namespace maxbl4.RfidCheckpointService.Services
{
    public interface IRfidService : IDisposable
    {
        void AppendRiderId(string riderId);
    }
}