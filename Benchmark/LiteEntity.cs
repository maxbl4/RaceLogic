using System;
using maxbl4.Race.Logic;

namespace Benchmark
{
    public class LiteEntityId
    {
        public Id<LiteEntityId> Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityGuid
    {
        public Guid Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityLong
    {
        public long Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
}