﻿using System;
using LiteDB;
using Newtonsoft.Json;

namespace maxbl4.Race.Services.CheckpointService.Model
{
    public class RfidOptions
    {
        public const string DefaultConnectionString = "Protocol=Alien;Network=127.0.0.1:20023"; 
        public static RfidOptions Default => new RfidOptions
        {
            ConnectionString = DefaultConnectionString,
            CheckpointAggregationWindowMs = 200
        };

        public int Id => 1;
        
        /// <summary>
        /// RfidDotnet connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Is Rfid reading enabled
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// In addition to checkpoints, persis the raw Tag objects streamed from the reader 
        /// </summary>
        public bool PersistTags { get; set; }
        /// <summary>
        /// Windows in milliseconds for tag deduplication
        /// </summary>
        public int CheckpointAggregationWindowMs { get; set; }
        /// <summary>
        /// Reads per second threshold below which a tag would be reported as bad 
        /// </summary>
        public int RpsThreshold { get; set; }
        public DateTime Timestamp { get; set; }
        
        public maxbl4.RfidDotNet.ConnectionString GetConnectionString() => RfidDotNet.ConnectionString.Parse(ConnectionString);
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}