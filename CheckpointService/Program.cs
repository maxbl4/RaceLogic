using System.Threading.Tasks;

namespace maxbl4.Race.CheckpointService
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var svc = new CheckpointServiceRunner();
            return await svc.Start(args);
        }
    }
}