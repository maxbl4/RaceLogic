using System;
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
}