using System.Threading.Tasks;

namespace TimingServices.Discovery
{
    public enum ServiceTypes
    {
        Input,
        RiderIdResolver,
        TimeSpanAggregation,
        RoundTracking,
        RoundScoring,
        ScoreAggregation
    }

    public class DiscoveryService
    {
        public Task RegisterService<T>(T service)
        {
            return Task.CompletedTask;
        }
    }
}