using System;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class MessageBase
    {
        public string MessageId { get; set; }
        public string SenderId { get; set; }
        public string TargetId { get; set; }
        public string MessageType { get; set; }
        
        public static MessageBase MaterializeConcreteMessage(JObject obj)
        {
            if (obj.TryGetValue(nameof(MessageType), StringComparison.OrdinalIgnoreCase, out var typeName))
            {
                var type = typeof(MessageBase).Assembly.GetType(typeName.ToString());
                if (type != null && type.IsSubclassOf(typeof(MessageBase)))
                    return (MessageBase)obj.ToObject(type);
            }   
            return obj.ToObject<MessageBase>();
        }
    }
}