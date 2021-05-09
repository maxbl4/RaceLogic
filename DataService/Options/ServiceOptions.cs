namespace maxbl4.Race.DataService.Options
{
    public class ServiceOptions
    {
        public string StorageConnectionString { get; set; }
        public string BraaapApiBaseUri { get; set; }
        public string BraaapApiKey { get; set; }
        public DataServiceOptions InitialDataServiceOptions { get; set; }
    }
}