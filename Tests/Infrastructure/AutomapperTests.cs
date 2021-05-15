using System;
using FluentAssertions;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class AutomapperTests
    {
        [Fact]
        public void Map_Checkpoint_to_dto()
        {
            var id = Guid.NewGuid();
            var mapper = new AutoMapperProvider();
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
    }
}