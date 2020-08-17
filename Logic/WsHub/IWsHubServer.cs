using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.Logic.WsHub
{
    public interface IWsHubServer
    {
        Task SendTo(JObject msg);
    }
}