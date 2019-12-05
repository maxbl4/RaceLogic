using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace maxbl4.Race.Logic.RiderIdResolving
{
    public interface IRiderIdResolver
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
        bool Resolve(string input, out string riderId);
        Task<string> ResolveCreateWhenMissing(string input);
    }

    public class SimpleMapRiderIdResolver : IRiderIdResolver
    {
        private readonly IDictionary<string, string> map;
        private readonly Func<string, Task<string>> createRiderId;

        public SimpleMapRiderIdResolver(IDictionary<string, string> map, Func<string, Task<string>> createRiderId)
        {
            this.map = map;
            this.createRiderId = createRiderId;
        }

        public bool Resolve(string input, out string riderId)
        {
            return map.TryGetValue(input, out riderId);
        }

        public async Task<string> ResolveCreateWhenMissing(string input)
        {
            if (!map.TryGetValue(input, out var riderId))
            {
                map[input] = riderId = await createRiderId(input);
            }

            return riderId;
        }
    }
}