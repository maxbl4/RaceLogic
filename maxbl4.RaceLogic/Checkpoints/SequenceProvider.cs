using System;
using System.Threading;

namespace maxbl4.RaceLogic.Checkpoints
{
    public static class SequenceProvider
    {
        private static long current = 0;
        public static long Next => Provider();
        public static Func<long> Provider = () => Interlocked.Increment(ref current);
    }
}