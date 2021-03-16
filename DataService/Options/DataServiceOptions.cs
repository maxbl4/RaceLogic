namespace maxbl4.Race.DataService.Options
{
    public class DataServiceOptions
    {
        public const string DefaultCheckpointsUri = "http://localhost:5050";

        public static DataServiceOptions Default => new()
        {
            CheckpointsUri = DefaultCheckpointsUri
        };

        public int Id => 1;
        public string CheckpointsUri { get; set; }
    }
}