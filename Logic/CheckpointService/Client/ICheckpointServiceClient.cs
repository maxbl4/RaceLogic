using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;

namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public interface ICheckpointServiceClient
    {
        ICheckpointSubscription CreateSubscription(DateTime from, TimeSpan? reconnectTimeout = null);
        Task<List<Checkpoint>> GetCheckpoints(DateTime? start = null, DateTime? end = null);
        Task AppendCheckpoint(string riderId);
        Task<int> DeleteCheckpoint(Id<Checkpoint> id);
        Task<int> DeleteCheckpoints(DateTime? start = null, DateTime? end = null);
        Task<RfidOptions> GetRfidOptions();
        Task SetRfidOptions(RfidOptions options);
        Task<T> GetRfidOptionsValue<T>(string property);
        Task SetRfidOptionsValue<T>(string property, T value);
        Task<bool> GetRfidStatus();
        Task SetRfidStatus(bool enabled);
    }
}