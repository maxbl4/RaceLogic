using System;
using System.Reactive.Subjects;
using RaceLogic.RiderIdResolving;

namespace RaceLogic.Checkpoints
{
    public class CheckpointProvider<TInput, TRiderId> : IObserver<TInput>, IObservable<Checkpoint<TRiderId>>
        where TRiderId: IEquatable<TRiderId>
    {
        private readonly IRiderIdResolver<TInput, TRiderId> riderIdResolver;
        private readonly Subject<Checkpoint<TRiderId>> checkpoints = new Subject<Checkpoint<TRiderId>>();

        public CheckpointProvider(IRiderIdResolver<TInput, TRiderId> riderIdResolver)
        {
            this.riderIdResolver = riderIdResolver;
        }

        public void OnCompleted()
        {
            checkpoints.OnCompleted();
        }

        public void OnError(Exception error)
        {
            checkpoints.OnError(error);
        }

        public async void OnNext(TInput value)
        {
            if (!riderIdResolver.Resolve(value, out var riderId))
            {
                riderId = await riderIdResolver.ResolveCreateWhenMissing(value);
            }
            checkpoints.OnNext(new Checkpoint<TRiderId>(riderId, SetTimestamp()));
        }

        public virtual DateTime? SetTimestamp()
        {
            return DateTime.UtcNow;
        }

        public IDisposable Subscribe(IObserver<Checkpoint<TRiderId>> observer)
        {
            return checkpoints.Subscribe(observer);
        }
    }
}