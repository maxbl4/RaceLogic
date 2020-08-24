namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class MessageTarget
    {
        public TargetType Type { get; set; }
        public string TargetId { get; set; }

        public override string ToString()
        {
            var prefix = Type switch
            {
                TargetType.Direct => "@",
                TargetType.Topic => "#",
                _ => "_"
            };
            return $"{prefix}{TargetId}";
        }
    }
}