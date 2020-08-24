namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class ServiceRegistration
    {
        public ServiceFeatures Features { get; set; }
        public string ServiceId { get; set; }

        public override string ToString()
        {
            return $"ServiceId: {ServiceId}, Features: {Features}";
        }
    }
}