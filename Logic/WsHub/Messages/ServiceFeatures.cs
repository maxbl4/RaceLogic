using System;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    [Flags]
    public enum ServiceFeatures
    {
        None = 0,
        CheckpointService = 1,
        RaceSession = 2,
        ManualInputTerminal = 4,
        LedScreen = 8
        /// etc
    }
}