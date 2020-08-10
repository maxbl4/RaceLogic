using LiteDB;

namespace maxbl4.Race.Extensions
{
    public static class LiteRepo
    {
        public static LiteRepository WithUtcDate(string connectionString)
        {
            return new LiteRepository(connectionString).WithUtcDate();
        }

        public static LiteRepository WithUtcDate(ConnectionString connectionString)
        {
            return new LiteRepository(connectionString).WithUtcDate();
        }

        public static LiteRepository WithUtcDate(this LiteRepository repo)
        {
            repo?.Database.Pragma("UTC_DATE", true);
            return repo;
        }
    }
}