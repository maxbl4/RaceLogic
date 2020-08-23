using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class Message
    {
        public Id<Message> MessageId { get; set; } = Id<Message>.NewId();
        public string SenderId { get; set; }
        public string TargetId { get; set; }
        public string MessageType { get; set; }
        
        public static Message MaterializeConcreteMessage(JObject obj)
        {
            if (obj.TryGetValue(nameof(MessageType), StringComparison.OrdinalIgnoreCase, out var typeName))
            {
                var type = typeof(Message).Assembly.GetType(typeName.ToString());
                if (type != null && type.IsSubclassOf(typeof(Message)))
                    return (Message)obj.ToObject(type);
            }   
            return obj.ToObject<Message>();
        }
    }

    public class RegisterServiceMessage
    {
        public ServiceFeatures Features { get; set; }
    }

    public class ListServiceRegistrationsRequest
    {
        
    }
    
    public class ListServiceRegistrationsResponse
    {
        public List<ServiceRegistration> Registrations { get; set; }
    }

    public class ServiceRegistration
    {
        public ServiceFeatures Features { get; set; }
        public string ServiceId { get; set; }

        public override string ToString()
        {
            return $"ServiceId: {ServiceId}, Features: {Features}";
        }
    }

    [Flags]
    public enum ServiceFeatures
    {
        None = 0,
        RfidReader = 1,
        RaceSession = 2,
        ManualInputTerminal = 4,
        LedScreen = 8
        /// etc
    }
}