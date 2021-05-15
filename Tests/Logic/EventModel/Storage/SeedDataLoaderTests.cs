using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LiteDB;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Tests.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.Logic.EventModel.Storage
{
    public class SeedDataLoaderTests: IntegrationTestBase
    {
        private readonly SeedDataLoaderOptions options;

        public SeedDataLoaderTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            options = new SeedDataLoaderOptions
            {
                SeedDataDirectory = outputHelper.GetWorkingDirectory("seed-data")
            };
        }

        [Fact]
        public void Test()
        {
            WithEventRepository((service, repository) =>
            {
                var loader = new SeedDataLoader(Options.Create(options), service, new ChannelMessageHub());
                Path.GetFileName(service.ConnectionString.Filename).Should().Be("_Test.litedb");
                loader.ResetAllData();
                Path.GetFileName(service.ConnectionString.Filename).Should().Be("_Test_001.litedb");
                var gates = service.List<GateDto>().ToList();
                gates.Should().HaveCount(1);
                gates[0].Name.Should().Be("Основные ворота");
                gates[0].Created.Should().BeCloseTo(DateTime.UtcNow, 5000);
            });
        }
    }
}