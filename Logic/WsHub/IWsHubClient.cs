using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub.Messages;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.Logic.WsHub
{
    public interface IWsHubClient
    {
        Task ReceiveMessage(Message message);
        Task InvokeRequest(Message message);
    }
}