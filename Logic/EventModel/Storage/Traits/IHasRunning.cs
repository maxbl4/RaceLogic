using System;

namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public interface IHasRunning : IHasTraits
    {
        DateTime StartTime { get; set; }
        DateTime StopTime { get; set; }
        bool IsRunning { get; set; }
    }
}