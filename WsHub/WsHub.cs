using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
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
        private static readonly ConcurrentDictionary<string, WsServiceRegistration>
            serviceRegistrations = new ConcurrentDictionary<string, WsServiceRegistration>();

        public void Register(RegisterServiceMessage msg)
        {
            lock(serviceRegistrations)
            {
                var reg = serviceRegistrations.GetOrAdd(Context.UserIdentifier, new WsServiceRegistration());
                reg.ServiceId = Context.UserIdentifier;
                reg.Features = msg.Features;
                reg.ConnectionIds.Add(Context.ConnectionId);
                logger.Information($"Service {Context.UserIdentifier} registered on {Context.ConnectionId} with features {msg.Features}" +
                                   $". Has {reg.ConnectionIds.Count} connections");
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            lock (serviceRegistrations)
            {
                if (serviceRegistrations.TryGetValue(Context.UserIdentifier, out var reg))
                {
                    reg.ConnectionIds.Remove(Context.ConnectionId);
                    logger.Information($"Service {Context.UserIdentifier} disconnected on {Context.ConnectionId}" +
                                       $". Has {reg.ConnectionIds.Count} connections");
                    if (reg.ConnectionIds.Count == 0)
                        serviceRegistrations.TryRemove(Context.UserIdentifier, out _);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        public ListServiceRegistrationsResponse ListServiceRegistrations(ListServiceRegistrationsRequest request)
        {
            return new ListServiceRegistrationsResponse
            {
                Registrations = serviceRegistrations.Values.Select(x => x.ToServiceRegistration()).ToList()
            };
        }

        public async Task Subscribe(TopicSubscribeMessage msg)
        {
            foreach (var topic in msg.TopicIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, topic);
            }
        }
        
        public async Task Unsubscribe(TopicSubscribeMessage msg)
        {
            foreach (var topic in msg.TopicIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
            }
        }

        public async Task SendTo(JObject obj)
        {
            var msg = Message.MaterializeConcreteMessage(obj);
            logger.Debug($"Message from {msg.SenderId} to {msg.Target}");
            switch (msg.Target.Type)
            {
                case TargetType.Direct:
                    var user = Clients.User(msg.Target.TargetId);
                    logger.Debug($"Resolved user {user}");
                    if (user == null)
                        throw new HubException($"User {msg.Target.TargetId} not found");
                    await user.ReceiveMessage(msg);
                    break;
                case TargetType.Topic:
                    var group = Clients.OthersInGroup(msg.Target.TargetId);
                    logger.Debug($"Resolved group {group}");
                    if (group == null)
                        throw new HubException($"Group {msg.Target.TargetId} not found");
                    await group.ReceiveMessage(msg);
                    break;
            }
        }
        
        private static readonly ConcurrentDictionary<Id<Message>, TaskCompletionSource<Message>> 
            outstandingClientRequests = new ConcurrentDictionary<Id<Message>, TaskCompletionSource<Message>>();
        
        public async Task<JObject> InvokeRequest(JObject obj)
        {
            var msg = Message.MaterializeConcreteMessage(obj);
            logger.Debug($"InvokeRequest Message from {msg.SenderId} to {msg.Target}");
            var user = Clients.User(msg.Target.TargetId);
            logger.Debug($"InvokeRequest Resolved user {user}");
            if (user == null)
                throw new HubException($"User {msg.Target.TargetId} not found");
            TaskCompletionSource<Message> tcs;
            outstandingClientRequests.TryAdd(msg.MessageId, tcs = new TaskCompletionSource<Message>());
            await user.InvokeRequest(msg);
            var result = await Task.WhenAny(Task.Delay(30000), tcs.Task);
            if (result is Task<Message> r)
                return JObject.FromObject(r.Result);
            throw new HubException($"Proxy call to {msg.Target} timed out {obj}", new TimeoutException());
        }

        public void AcceptResponse(JObject obj)
        {
            var msg = Message.MaterializeConcreteMessage(obj);
            logger.Debug($"AcceptResponse Message from {Context.UserIdentifier} to {msg.Target}");
            if (outstandingClientRequests.TryGetValue(msg.MessageId, out var tcs))
                tcs.TrySetResult(msg);
            else
                tcs.TrySetCanceled();
        }
    }
}