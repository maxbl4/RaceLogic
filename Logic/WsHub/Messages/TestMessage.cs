namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class TestMessage : Message
    {
        public string Payload { get; set; }
    }
    
    public class TestRequest : RequestMessage
    {
        public string Payload { get; set; }
    }
}