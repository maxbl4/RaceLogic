using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.Race.Logic.RiderIdResolving;

namespace maxbl4.Race.Logic.Checkpoints
{
    public class CheckpointProvider : IObservable<Checkpoint>
    {
        private readonly IRiderIdResolver riderIdResolver;
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();

        public CheckpointProvider(IRiderIdResolver riderIdResolver)
        {
            this.riderIdResolver = riderIdResolver;
        }

        public async Task ProvideInput(string value, DateTime? timeStamp = null)
        {
            var riderId = await riderIdResolver.Resolve(value);
            checkpoints.OnNext(new Checkpoint(riderId, timeStamp));
        }

        public IDisposable Subscribe(IObserver<Checkpoint> observer)
        {
            return checkpoints.Subscribe(observer);
        }
    }
}