using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RaceLogic.RiderIdResolving;

namespace maxbl4.RaceLogic.Checkpoints
{
    public class CheckpointProvider<TInput, TRiderId> : IObservable<Checkpoint<TRiderId>>
        where TRiderId: IEquatable<TRiderId>
    {
        private readonly IRiderIdResolver<TInput, TRiderId> riderIdResolver;
        private readonly Subject<Checkpoint<TRiderId>> checkpoints = new Subject<Checkpoint<TRiderId>>();

        public CheckpointProvider(IRiderIdResolver<TInput, TRiderId> riderIdResolver)
        {
            this.riderIdResolver = riderIdResolver;
        }

        public async Task ProvideInput(TInput value, DateTime? timeStamp = null)
        {
            if (!riderIdResolver.Resolve(value, out var riderId))
            {
                riderId = await riderIdResolver.ResolveCreateWhenMissing(value);
            }
            checkpoints.OnNext(new Checkpoint<TRiderId>(riderId, timeStamp));
        }

        public IDisposable Subscribe(IObserver<Checkpoint<TRiderId>> observer)
        {
            return checkpoints.Subscribe(observer);
        }
    }
}