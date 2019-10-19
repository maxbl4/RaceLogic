using System.Collections.Generic;
using LiteDB;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Rfid;

namespace maxbl4.RfidCheckpointService.Services
{
    public class StorageService
    {
        public const string DefaultDatabaseFilename = "storage.litedb";
        private readonly ConnectionString connectionString; 

        public StorageService() : this(new ConnectionString
        {
            Filename = DefaultDatabaseFilename,
            InitialSize = 1024 * 1024 * 10, //10Mb
            UtcDate = true
        }){}
        
        public StorageService(ConnectionString connectionString)
        {
            this.connectionString = connectionString;
            SetupIndexes();
        }

        private void SetupIndexes()
        {
            using var repo = new LiteRepository(connectionString);
            repo.Database.GetCollection<Checkpoint>().EnsureIndex(x => x.Timestamp);
        }

        public void AppendCheckpoint(Checkpoint cp)
        {
            using var repo = new LiteRepository(connectionString);
            repo.Insert(cp);
        }

        public List<Checkpoint> ListCheckpoints()
        {
            using var repo = new LiteRepository(connectionString);
            return repo.Query<Checkpoint>().ToList();
        }

        public RfidSettings GetRfidSettings()
        {
            using var repo = new LiteRepository(connectionString);
            return repo.Query<RfidSettings>().FirstOrDefault() ?? RfidSettings.Default;
        }
        
        public void SetRfidSettings(RfidSettings rfidSettings)
        {
            using var repo = new LiteRepository(connectionString);
            repo.Upsert(rfidSettings);
        }
    }
}