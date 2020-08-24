using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class Message
    {
        public Id<Message> MessageId { get; set; } = Id<Message>.NewId();
        public string SenderId { get; set; }
        public MessageTarget Target { get; set; }
        public string MessageType { get; set; }

        protected Message()
        {
            MessageType = GetType().FullName;
        }

        public static T MaterializeConcreteMessage<T>(JObject obj)
            where T: Message
        {
            if (obj.TryGetValue(nameof(MessageType), StringComparison.OrdinalIgnoreCase, out var typeName))
            {
                var type = typeof(Message).Assembly.GetType(typeName.ToString());
                if (type != null && type.IsSubclassOf(typeof(Message)))
                    return (T)obj.ToObject(type);
            }   
            return obj.ToObject<T>();
        }
        
        public static Message MaterializeConcreteMessage(JObject obj)
        {
            return MaterializeConcreteMessage<Message>(obj);
        }
    }
}