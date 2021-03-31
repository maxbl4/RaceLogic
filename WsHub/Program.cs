using System.Threading.Tasks;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.WsHub
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var svc = new ServiceRunner<Startup>();
            return await svc.Start(args);
        }
    }
}