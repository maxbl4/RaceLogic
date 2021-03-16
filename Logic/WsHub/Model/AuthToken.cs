using System;
using LiteDB;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.WsHub.Model
{
    public class AuthToken : IHasTimestamp
    {
        [BsonId] public string Token { get; set; }

        public string ServiceName { get; set; }
        public string Roles { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}