using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using Serilog;

namespace maxbl4.Race.Logic.WsHub
{
    public class WsHubConnectionService : IAsyncInitialize, IAsyncDisposable
    {
        private readonly ILogger logger = Log.ForContext<WsHubConnectionService>();
        public ConcurrentDictionary<Type, Func<IRequestMessage, Task<Message>>> RequestHandlers { get; } = new();

        public WsHubConnection Connection { get; set; }

        public ValueTask DisposeAsync()
        {
            return default;
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
                Connection.RequestHandlers[requestHandler.Key] = requestHandler.Value;

            _ = Connection.Connect();
        }

        public void RegisterRequestHandler<T>(Func<T, Task<Message>> handler)
            where T : Message, IRequestMessage
        {
            RequestHandlers[typeof(T)] = msg => handler((T) msg);
        }
    }
}