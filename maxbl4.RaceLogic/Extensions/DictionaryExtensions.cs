using System;
using System.Collections.Generic;

namespace maxbl4.RaceLogic.Extensions
{
    public static class DictionaryExtensions
    {
        public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default)
        {
            TV val;
            if (dict.TryGetValue(key, out val))
                return val;
            return defaultValue;
        }

        public static TV GetOrAdd<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TK,TV> valueFactory)
        {
            TV val;
            if (dict.TryGetValue(key, out val))
                return val;
            dict[key] = val = valueFactory(key);
            return val;
        }

        public static TV UpdateOrAdd<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TV, TV> updateFunc, TV defaultValue = default)
        {
            TV val;
            if (!dict.TryGetValue(key, out val))
                val = defaultValue;
            val = updateFunc(val);
            dict[key] = val;
            return val;
        }
    }
}