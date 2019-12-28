using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;

namespace Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [RPlotExporter, RankColumn]
    public class TrackOfCheckpointsBench
    {
        [Params(10, 100)]
        public int N;

        private InstanceRunner runner;
        private InstanceRunner runnerOld;

        [GlobalSetup]
        public void Setup()
        {
            runner = new InstanceRunner(() => new TrackOfCheckpoints(new DateTime(1), FinishCriteria.FromForcedFinish()));
            runnerOld = new InstanceRunner(() => new TrackOfCheckpointsOld(new DateTime(1), FinishCriteria.FromForcedFinish()));
        }

        [Benchmark]
        public void OldTrack() => runnerOld.Work(N);
        
        [Benchmark]
        public void Track() => runner.Work(N);
    }

    public class InstanceRunner
    {
        private readonly Func<ITrackOfCheckpoints> trackFactory;
        private readonly Random r = new Random(1);
        private long timestamp = 1000000;
        public InstanceRunner(Func<ITrackOfCheckpoints> trackFactory)
        {
            this.trackFactory = trackFactory;
        }

        public void Work(int n)
        {
            var track = trackFactory();
            for (var i = 0; i < n; i++)
            {
                Next(track);
            }
        }

        void Next(ITrackOfCheckpoints track)
        {
            track.Append(new Checkpoint(r.Next(1, 100).ToString(), new DateTime(timestamp += 10000)));
        }
    } 
}