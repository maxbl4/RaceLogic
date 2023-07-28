using System;
using System.Collections.Generic;

namespace maxbl4.Race.Logic.Extensions;

public static class IEnumerableExt
{
    public static void ForAll<T>(this IEnumerable<T> t, Action<T> action)
    {
        foreach (var v in t)
        {
            action(v);
        }
    }
}