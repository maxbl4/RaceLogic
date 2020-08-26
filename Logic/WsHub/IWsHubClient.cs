using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.Logic.WsHub
{
    public interface IWsHubClient
    {
        Task ReceiveMessage(Message message);
        Task InvokeRequest(Message message);
    }
}