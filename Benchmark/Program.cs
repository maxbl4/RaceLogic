using System;
using System.Diagnostics;
using maxbl4.Race.Logic.RoundTiming;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("TrackOfCheckpoints benchmark. Supply number of checkpoints in one cycle and number of cycles");
                Console.WriteLine("Example: benchmark.exe 1000 100");
                return;
            }
            var cps = int.Parse(args[0]);
            var cycles = int.Parse(args[1]);
            Console.WriteLine($"TrackOfCheckpoints benchmark. Append {cps} checkpoints {cycles} times");
            var incrementalWithCustomSortRunner = new InstanceRunner(() => new TrackOfCheckpoints(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var incrementalRunner = new InstanceRunner(() => new TrackOfCheckpointsIncremental(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var cyclicRunner = new InstanceRunner(() => new TrackOfCheckpointsCyclic(new DateTime(1), FinishCriteria.FromForcedFinish()));
            var runners = new[] { ("cyclic", cyclicRunner), ("Incremental", incrementalRunner), ("Incremental custom sort", incrementalWithCustomSortRunner)};
            foreach (var runner in runners)
            {
                runner.Item2.Work(100);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Start test: {runner.Item1}");
                Console.ForegroundColor = ConsoleColor.Gray;
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < cycles; i++)
                {
                    runner.Item2.Work(cps);
                    if (i % 10 == 0)
                        Console.Write($"{i / cycles * 100}% ");
                }
                sw.Stop();
                Console.WriteLine($"100%");
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{runner.Item1}: {sw.ElapsedMilliseconds} ms per 100 iterations");
            }
        }
    }
}