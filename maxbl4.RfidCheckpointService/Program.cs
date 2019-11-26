using System.Threading.Tasks;

namespace maxbl4.RfidCheckpointService
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var svc = new RfidCheckpointServiceRunner();
            return await svc.Start(args);
        }
    }
}