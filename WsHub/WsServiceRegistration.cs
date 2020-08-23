using System.Collections.Generic;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.WsHub
{
    public class WsServiceRegistration: ServiceRegistration
    {
        public HashSet<string> ConnectionIds { get; set;  } = new HashSet<string>();

        public ServiceRegistration ToServiceRegistration()
        {
            return new ServiceRegistration
            {
                ServiceId = ServiceId,
                Features = Features
            };
        }
    }
}