namespace maxbl4.RfidCheckpointService
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            using var svc = new RfidCheckpointServiceRunner();
            return svc.Start(args).Result;
        }
    }
}