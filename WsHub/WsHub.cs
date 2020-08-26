using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public static readonly ConcurrentDictionary<Id<Message>, TaskCompletionSource<Message>> 
            OutstandingClientRequests = new ConcurrentDictionary<Id<Message>, TaskCompletionSource<Message>>();

        public async Task Register(RegisterServiceMessage msg)
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
            var msg = MaterializeMessage<Message>(obj);
            var target = ResolveTarget(msg);
            await target.ReceiveMessage(msg);
        }
        
        public async Task<JObject> InvokeRequest(JObject obj)
        {
            var msg = MaterializeMessage<RequestMessage>(obj);
            var target = ResolveTarget(msg);
            try
            {
                TaskCompletionSource<Message> tcs;
                OutstandingClientRequests.TryAdd(msg.MessageId, tcs = new TaskCompletionSource<Message>());
                _ = target.InvokeRequest(msg);
                var result = await Task.WhenAny(Task.Delay(msg.Timeout), tcs.Task);
                if (result is Task<Message> r)
                    return JObject.FromObject(r.Result);
                throw new HubException($"Proxy call to {msg.Target} timed out {obj}");
            }
            finally
            {
                OutstandingClientRequests.TryRemove(msg.MessageId, out _);
            }
        }

        public async Task AcceptResponse(JObject obj)
        {
            var msg = MaterializeMessage<Message>(obj);
            logger.Debug($"AcceptResponse {obj}");
            if (OutstandingClientRequests.TryGetValue(msg.MessageId, out var tcs))
                tcs.TrySetResult(msg);
        }

        IWsHubClient ResolveTarget(Message msg, [CallerMemberName] string methodName = null)
        {
            IWsHubClient client = null;
            switch (msg.Target.Type)
            {
                case TargetType.Direct:
                    client = Clients.User(msg.Target.TargetId);
                    break;
                case TargetType.Topic:
                    client = Clients.OthersInGroup(msg.Target.TargetId);
                    break;
            }

            if (client == null) throw new HubException($"Could not resolve target {msg.Target}");
            logger.Debug($"Resolved {msg.Target} into {client}");
            return client;
        }

        T MaterializeMessage<T>(JObject obj, [CallerMemberName]string methodName = null)
            where T: Message
        {
            try
            {
                var msg = Message.MaterializeConcreteMessage<T>(obj);
                msg.SenderId = Context.UserIdentifier;
                logger.Debug($"{methodName} materialized {msg.GetType().Name} from {Context.UserIdentifier} to {msg.Target}");
                return msg;
            }
            catch (Exception ex)
            {
                throw new HubException($"Failed to materialize from {Context.UserIdentifier} {obj} {ex}");
            }
        }
    }
}