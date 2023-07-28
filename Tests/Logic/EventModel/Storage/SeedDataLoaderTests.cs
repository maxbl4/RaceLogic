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
using JsonSerializer = LiteDB.JsonSerializer;

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
                loader.Load(false);
                var events = service.List<EventDto>().ToList();
                events.Should().HaveCount(1);
                var ev = events[0];
                ev.Id.Should().Be(new Guid("08d91d00795fca4b870eae38900278be"));
                ev.Name.Should().Be("Тестовая гонка");
                ev.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
                ev.Name = "some";
                service.Save(ev);
                loader.Load(false);
                service.List<EventDto>().Single().Name.Should().Be("some");
                loader.Load(true);
                service.List<EventDto>().Single().Name.Should().Be("Тестовая гонка");
            });
        }
    }
}