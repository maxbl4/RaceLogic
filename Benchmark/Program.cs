using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using maxbl4.Race.Logic.RoundTiming;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var incrementalRunner = new InstanceRunner(() => new TrackOfCheckpoints(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var cyclicRunner = new InstanceRunner(() => new TrackOfCheckpointsOld(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var runners = new[] { ("Incremental", incrementalRunner), ("cyclic", cyclicRunner)};
            foreach (var runner in runners)
            {
                Console.Write($".");
                runner.Item2.Work(100);
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++)
                {
                    runner.Item2.Work(10000);
                    Console.Write($".");
                }
                sw.Stop();
                Console.WriteLine($".");
                Console.WriteLine($"{runner.Item1}: {sw.Elapsed}");
            }
        }
    }
}