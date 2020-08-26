using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;

namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public interface ICheckpointSubscription : IDisposable
    {
        IObservable<Checkpoint> Checkpoints { get; }
        IObservable<ReaderStatus> ReaderStatus { get; }
        IObservable<WsConnectionStatus> WebSocketConnected { get; }
        void Start();
    }
}