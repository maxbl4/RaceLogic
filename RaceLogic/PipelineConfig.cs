using System;
using System.Collections.Generic;
using System.IO;
using RaceLogic.Extensions;
using RaceLogic.Pipeline;

namespace RaceLogic
{
    public class PipelineConfig<TRiderId>
        where TRiderId: IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        readonly Dictionary<Type, object> inputMaps = new Dictionary<Type, object>();
        public void SetInputMap<TInput>(IInputMap<TInput, TRiderId> input)
        {
            inputMaps[typeof(TInput)] = input;
        }

        public void SetCheckpointAggregator()
        {
            
        }

        public bool ProvideInput<TInput>(TInput input)
        {
            var map = inputMaps.Get(typeof(TInput)) as IInputMap<TInput, TRiderId>;
            if (map == null) return false;
            var checkpoints = map.Map(input);
            return false;
        }
    }
}