using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.Logic.WsHub
{
    public interface IRequestImpl<in TRequest,TResponse>
        where TRequest: Message, IRequestMessage
        where TResponse: Message
    {
        private WsHubConnection Connection => this as WsHubConnection;

        public Task<TResponse> Invoke(string targetId, TRequest request)
        {
            return Connection.InvokeRequest<TRequest, TResponse>(targetId, request);
        }
    }

    public interface IPingRequester : IRequestImpl<PingRequest, PingResponse>
    {
        
    }
}