using System;

namespace maxbl4.Race.CheckpointService.Services
{
    public interface IRfidService : IDisposable
    {
        void AppendRiderId(string riderId);
    }
}