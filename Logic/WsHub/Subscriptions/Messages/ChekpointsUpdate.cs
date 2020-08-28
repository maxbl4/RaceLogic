using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.Logic.WsHub.Subscriptions.Messages
{
    public class ChekpointsUpdate: Message, IRequestMessage
    {
        public RfidOptions RfidOptions { get; set; }
        public ReaderStatus ReaderStatus { get; set; }
        public IList<Checkpoint> Checkpoints { get; set; }
        public TimeSpan? Timeout { get; set; }
    }

    public class ChekpointsUpdateResponse : Message
    {
        
    }
}