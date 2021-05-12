using System;
using System.Reactive.Subjects;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.CheckpointService.Model;
using Serilog;

namespace maxbl4.Race.Tests.CheckpointService.Client
{
    public class FakeCheckpointSubscription: ICheckpointSubscription
    {
        private static readonly ILogger logger = Log.ForContext<FakeCheckpointSubscription>();
        public DateTime Now = DateTime.UtcNow;
        public Subject<Checkpoint> CheckpointsSubject { get; } = new();
        public Subject<ReaderStatus> ReaderStatusSubject { get; } = new();
        public Subject<WsConnectionStatus> WebSocketConnectedSubject { get; } = new();

        public void SendTags(params (int time, string tag)[] tags)
        {
            foreach (var (time, tag) in tags)
            {
                logger.Information("Sending tag {tag} {timestamp}", tag, Now.AddSeconds(time));
                CheckpointsSubject.OnNext(new Checkpoint(tag, Now.AddSeconds(time)));
            }
        }
        
        public void Dispose()
        {
        }

        public IObservable<Checkpoint> Checkpoints => CheckpointsSubject;
        public IObservable<ReaderStatus> ReaderStatus => ReaderStatusSubject;
        public IObservable<WsConnectionStatus> WebSocketConnected => WebSocketConnectedSubject;
        public void Start()
        {
        }
    }
}