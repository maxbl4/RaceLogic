using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub.Subscriptions;

namespace maxbl4.Race.Logic.WsHub
{
    public interface IUpstreamOptionsStorage
    {
        Task<UpstreamOptions> GetUpstreamOptions();
        Task SetUpstreamOptions(UpstreamOptions options);
    }
}