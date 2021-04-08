namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public class CheckpointServiceClientFactory : ICheckpointServiceClientFactory
    {
        public ICheckpointServiceClient CreateClient(string address)
        {
            return new CheckpointServiceClient(address);
        }
    }
}