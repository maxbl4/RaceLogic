using System.Collections.Concurrent;
using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Serilog;

namespace maxbl4.Race.WsHub
{
    [Authorize]
    public class WsHub: Hub<IWsHubClient>, IWsHubServer
    {
        private static readonly ILogger logger = Log.ForContext<WsHub>();

        public async Task SendTo(JObject obj)
        {
            var msg = MessageBase.MaterializeConcreteMessage(obj);
            logger.Debug($"Message from {msg.SenderId} to {msg.TargetId}");
            var user = Clients.User(msg.TargetId);
            logger.Debug($"Resolved user {user}");
            await user.ReceiveMessage(msg);
        }
    }
}