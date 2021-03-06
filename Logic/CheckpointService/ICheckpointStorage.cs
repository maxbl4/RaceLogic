using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.CheckpointService
{
    public interface ICheckpointStorage
    {
        List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null);
    }
}