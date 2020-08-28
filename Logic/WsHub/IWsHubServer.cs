using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub.Messages;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.Logic.WsHub
{
    public interface IWsHubServer
    {
        Task SendTo(JObject msg);
        void Register(RegisterServiceMessage msg);
        ListServiceRegistrationsResponse ListServiceRegistrations(ListServiceRegistrationsRequest request);
        Task Subscribe(TopicSubscribeMessage msg);
        Task Unsubscribe(TopicSubscribeMessage msg);
    }
}