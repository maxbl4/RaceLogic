using System;

namespace maxbl4.Race.Logic.CheckpointService
{
    public interface IRfidService : IDisposable
    {
        void AppendRiderId(string riderId);
    }
}