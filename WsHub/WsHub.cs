using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace maxbl4.Race.WsHub
{
    [Authorize]
    public class WsHub: Hub<IWsHubClient>
    {
        private static readonly ILogger logger = Log.ForContext<WsHub>();

        public override Task OnConnectedAsync()
        {
            logger.Debug($"New Connection {Context.ConnectionId} {Context.UserIdentifier} {Context.User?.Identity?.Name}");
            return base.OnConnectedAsync();
        }

        public async Task SendTo(MessageBase msg)
        {
            logger.Debug($"Message from {msg.SenderId} to {msg.TargetId}");
            var user = Clients.User(msg.TargetId);
            logger.Debug($"Resolved user {user}");
            await user.ReceiveMessage(msg);
        }

    }

    public class RegisterMessage
    {
        public string ClientId { get; set; }
    }

    public class ClientRegistration
    {
        public string ClientId { get; set; }
        public ConcurrentBag<string> ConnectionIds { get; set; } = new ConcurrentBag<string>();
    }

    public interface IWsHubClient
    {
        Task ReceiveMessage(MessageBase messageBase);
    }

    public class MessageBase
    {
        public string MessageId { get; set; }
        public string SenderId { get; set; }
        public string TargetId { get; set; }
        public string Payload { get; set; }
    }
}