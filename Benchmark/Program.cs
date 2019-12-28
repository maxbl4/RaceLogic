using System;
using System.Diagnostics;
using maxbl4.Race.Logic.RoundTiming;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var incrementalWithCustomSortRunner = new InstanceRunner(() => new TrackOfCheckpoints(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var incrementalRunner = new InstanceRunner(() => new TrackOfCheckpointsIncremental(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var cyclicRunner = new InstanceRunner(() => new TrackOfCheckpointsCyclic(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var runners = new[] { ("cyclic", cyclicRunner), ("Incremental", incrementalRunner), ("Incremental custom sort", incrementalWithCustomSortRunner)};
            foreach (var runner in runners)
            {
                runner.Item2.Work(100);
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++)
                {
                    runner.Item2.Work(1000);
                    if (i % 10 == 0)
                        Console.Write($"{i}% ");
                }
                sw.Stop();
                Console.WriteLine($"100%");
                Console.WriteLine($"{runner.Item1}: {sw.ElapsedMilliseconds} ms per 100 iterations");
            }
        }
    }
}