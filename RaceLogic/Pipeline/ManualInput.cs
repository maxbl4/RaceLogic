using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Checkpoints;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;
using RaceLogic.Model;
using RaceLogic.ReferenceModel;

namespace RaceLogic.Pipeline
{
    public interface IPipelineInput<TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        IObservable<Checkpoint<TRiderId>> Checkpoints { get; }  
    }
    
    public class ManualInput<TRiderId>: IPipelineInput<TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        public IObservable<Checkpoint<TRiderId>> Checkpoints { get; }  
    }

    public interface IInputMap<in TInput, TRiderId> where TRiderId : IEquatable<TRiderId>
    {
        IEnumerable<Checkpoint<TRiderId>> Map(TInput input);
    }

    public class InputMap<TInput, TRiderId> : IInputMap<TInput, TRiderId> where TRiderId: IEquatable<TRiderId>
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