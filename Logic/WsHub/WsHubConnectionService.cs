using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.Extensions.TaskExt;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using maxbl4.Race.Logic.WsHub.Subscriptions.Messages;
using Serilog;
using Serilog.Core;

namespace maxbl4.Race.Logic.WsHub
{
    public class WsHubConnectionService: IAsyncInitialize, IAsyncDisposable
    {
        private readonly ILogger logger = Log.ForContext<WsHubConnectionService>();
        private WsHubConnection wsConnection;
        public ConcurrentDictionary<Type, Func<IRequestMessage, Task<Message>>> RequestHandlers { get; } = new ConcurrentDictionary<Type, Func<IRequestMessage, Task<Message>>>();

        public WsHubConnection Connection
        {
            get => wsConnection;
            set => wsConnection = value;
        }

        public Task InitializeAsync()
        {
            return null;
        }
        
        private async Task OptionsChanged(UpstreamOptions options)
        {
            options.ConnectionOptions.Features |= ServiceFeatures.CheckpointService;
            if (Connection != null)
                await logger.Swallow(async () => await Connection.DisposeAsync());
            Connection = new WsHubConnection(options.ConnectionOptions);
            foreach (var requestHandler in RequestHandlers)
            {
                Connection.RequestHandlers[requestHandler.Key] = requestHandler.Value;    
            }
            
            _ = Connection.Connect();
        }
        
        public void RegisterRequestHandler<T>(Func<T, Task<Message>> handler)
            where T: Message, IRequestMessage
        {
            RequestHandlers[typeof(T)] = msg => handler((T)msg);
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }
}