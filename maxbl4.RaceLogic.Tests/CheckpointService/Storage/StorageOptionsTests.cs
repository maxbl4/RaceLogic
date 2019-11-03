﻿using LiteDB;
using maxbl4.RfidCheckpointService.Services;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Storage
{
    public class StorageOptionsTests
    {
        [Fact]
        public void ShouldSerializeAndDeserialize()
        {
            var options = new StorageOptions
                {ConnectionString = "Filename=storage.litedb;InitialSize=123;UtcDate=true"};
            var s = System.Text.Json.JsonSerializer.Serialize(options);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<StorageOptions>(s);
            deserialized.ShouldNotBeSameAs(options);
            deserialized.ConnectionString.ShouldBe("Filename=storage.litedb;InitialSize=123;UtcDate=true");
        }
    }
}