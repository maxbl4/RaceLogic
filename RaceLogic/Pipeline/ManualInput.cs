using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;
using RaceLogic.Model;
using RaceLogic.ReferenceModel;

namespace RaceLogic.Pipeline
{
    public interface IPipelineInput<out TRiderId>
        where TRiderId: IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        IObservable<ICheckpoint<TRiderId>> Checkpoints { get; }  
    }
    
    public class ManualInput<TRiderId>: IPipelineInput<TRiderId>
        where TRiderId: IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        public IObservable<ICheckpoint<TRiderId>> Checkpoints { get; }  
    }

    public interface IInputMap<in TInput, TRiderId> where TRiderId : IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        IEnumerable<Checkpoint<TRiderId>> Map(TInput input);
    }

    public class InputMap<TInput, TRiderId> : IInputMap<TInput, TRiderId> where TRiderId: IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        private readonly IDictionary<TInput, IEnumerable<TRiderId>> initialMap;

        public InputMap(IDictionary<TInput, IEnumerable<TRiderId>> initialMap)
        {
            this.initialMap = initialMap;
        }

        public IEnumerable<Checkpoint<TRiderId>> Map(TInput input)
        {
            return initialMap.Get(input, new TRiderId[0])
                .Select(x => new Checkpoint<TRiderId>(x, DateTime.UtcNow));
        }
    }
}