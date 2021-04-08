namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public interface ICheckpointServiceClientFactory
    {
        ICheckpointServiceClient CreateClient(string address);
    }
}