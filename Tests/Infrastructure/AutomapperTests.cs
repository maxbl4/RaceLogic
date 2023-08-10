using System;
using FluentAssertions;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class AutomapperTests
    {
        AutoMapperProvider mapper = new AutoMapperProvider();
        [Fact]
        public void Map_Checkpoint_to_dto()
        {
            var id = Guid.NewGuid();
            var dto = mapper.Map<CheckpointDto>(new Checkpoint
            {
                Id = id,
                Count = 5
            });
            dto.Id.Value.Should().Be(id);
            dto.Count.Should().Be(5);
            
            var cp = mapper.Map<Checkpoint>(dto);
            cp.Id.Value.Should().Be(id);
        }
        
        [Fact]
        public void Map_Lap_to_webmodel()
        {
            var roundStart = DateTime.UtcNow;
            var lap = new Lap(new Checkpoint("123",
                    DateTime.UtcNow.AddMinutes(1)),
                roundStart);
            var dto = mapper.Map<maxbl4.Race.Logic.WebModel.Lap>(lap);
            dto.Duration.Should().BeCloseTo(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(1));
            dto.Start.Should().BeCloseTo(roundStart, TimeSpan.FromSeconds(1));
            dto.End.Should().BeCloseTo(roundStart.AddMinutes(1), TimeSpan.FromSeconds(1));
        }
    }
}