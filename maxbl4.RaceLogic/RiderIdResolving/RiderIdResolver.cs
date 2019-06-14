using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace maxbl4.RaceLogic.RiderIdResolving
{
    public interface IRiderIdResolver<TInput, TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        /// <summary>
        /// Should be used to resolve RiderId from input data, e.g. from number entered manually or RFID tag string.
        ///  This method should be implemented as quick in-memory lookup and work for most real-life inputs.
        /// E.g. numbers of riders are know prior to race and can be put input a map.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="riderId"></param>
        /// <returns>True if successfully resolved. In case False was returned, ResolveCreateWhenMissing method should be called.
        /// Depending on implementation, method can create new record in database, thus it should be implemented as async Task.</returns>
        bool Resolve(TInput input, out TRiderId riderId);
        Task<TRiderId> ResolveCreateWhenMissing(TInput input);
    }

    public class SimpleMapRiderIdResolver<TInput, TRiderId> : IRiderIdResolver<TInput, TRiderId> where TRiderId : IEquatable<TRiderId>
    {
        private readonly IDictionary<TInput, TRiderId> map;
        private readonly Func<TInput, Task<TRiderId>> createRiderId;

        public SimpleMapRiderIdResolver(IDictionary<TInput, TRiderId> map, Func<TInput, Task<TRiderId>> createRiderId)
        {
            this.map = map;
            this.createRiderId = createRiderId;
        }

        public bool Resolve(TInput input, out TRiderId riderId)
        {
            return map.TryGetValue(input, out riderId);
        }

        public async Task<TRiderId> ResolveCreateWhenMissing(TInput input)
        {
            if (!map.TryGetValue(input, out var riderId))
            {
                map[input] = riderId = await createRiderId(input);
            }

            return riderId;
        }
    }
}