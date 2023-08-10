﻿using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;
using maxbl4.Race.Logic.WebModel;
using Xunit;
using Lap = maxbl4.Race.Logic.RoundTiming.Lap;
using RoundPosition = maxbl4.Race.Logic.RoundTiming.RoundPosition;

namespace maxbl4.Race.Tests.Logic;

public class AutoMapperTests
{
    private readonly AutoMapperProvider mapper;

    public AutoMapperTests()
    {
        mapper = new AutoMapperProvider();
    }
    
    [Fact]
    public void MapTimingSessionUpdate()
    {
        var rating = new List<RoundPosition>();
        rating.Add(RoundPosition.FromLaps("1", new []
        {
            new Lap(new Checkpoint("1", 1), DateTime.UtcNow)
        }, true));
        var update = TimingSessionUpdate.From(Id<TimingSessionDto>.NewId(), rating, mapper);
    }
}