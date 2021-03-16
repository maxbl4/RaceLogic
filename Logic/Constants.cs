using System;

namespace maxbl4.Race.Logic
{
    public class Constants
    {
        public static readonly DateTime DefaultUtcDate = new(0, DateTimeKind.Utc);
        public class WsHub
        {
            public class Authentication
            {
                public const string SchemeName = "WsAccessToken";    
            }

            public class Roles
            {
                public const string Admin = "Admin";
            }
        }
    }
}