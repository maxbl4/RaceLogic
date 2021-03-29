using System;
using Newtonsoft.Json.Linq;
using SequentialGuid;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public interface IMessage
    {
        Guid MessageId { get; set; }
        string SenderId { get; set; }
        MessageTarget Target { get; set; }
        string MessageType { get; set; }
    }

    public class Message : IMessage
    {
        protected Message()
        {
            MessageType = GetType().FullName;
        }

        public Guid MessageId { get; set; } = SequentialGuidGenerator.Instance.NewGuid();
        public string SenderId { get; set; }
        public MessageTarget Target { get; set; }
        public string MessageType { get; set; }

        public static T MaterializeConcreteMessage<T>(JObject obj)
            where T : Message
        {
            if (obj.TryGetValue(nameof(MessageType), StringComparison.OrdinalIgnoreCase, out var typeName))
            {
                var type = typeof(Message).Assembly.GetType(typeName.ToString());
                if (type != null && type.IsSubclassOf(typeof(Message)))
                    return (T) obj.ToObject(type);
            }

            return obj.ToObject<T>();
        }

        public static Message MaterializeConcreteMessage(JObject obj)
        {
            return MaterializeConcreteMessage<Message>(obj);
        }
    }
}