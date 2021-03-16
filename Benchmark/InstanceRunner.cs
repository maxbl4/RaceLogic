using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;

namespace Benchmark
{
    public class InstanceRunner
    {
        private readonly Random r = new(1);
        private readonly Func<ITrackOfCheckpoints> trackFactory;
        private long timestamp = 1000000;

        public InstanceRunner(Func<ITrackOfCheckpoints> trackFactory)
        {
            this.trackFactory = trackFactory;
        }

        public void Work(int n)
        {
            var track = trackFactory();
            for (var i = 0; i < n; i++) Next(track);
        }

        private void Next(ITrackOfCheckpoints track)
        {
            track.Append(new Checkpoint(r.Next(1, 100).ToString(), new DateTime(timestamp += 10000)));
            var t = track.Rating;
        }
    }
}