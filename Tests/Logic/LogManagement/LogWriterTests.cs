using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.LogManagement.EntryTypes;
using maxbl4.Race.Logic.LogManagement.IO;
using Xunit;

namespace maxbl4.Race.Tests.Logic.LogManagement
{
    public class LogWriterTests
    {
        private readonly string simpleLog1 =
            @"{'$type':'start','Duration':'00:45:00','Timestamp':'0001-01-01T00:00:00.1','Id':1}
{'$type':'cp','Timestamp':'0001-01-01T00:00:00.11','RiderId':'xxxx','Id':2,'Count':1}
{'$type':'drop','Timestamp':'0001-01-01T00:00:00.12','Id':3}
".Replace('\'', '"');
        
        [Fact]
        public void Should_write_logs()
        {
            var sw = new StringWriter();
            
            var logWriter = new LogWriter(sw);
            logWriter.Append(new SessionStart {
                Timestamp = new DateTime(1000000),
                Duration = TimeSpan.FromMinutes(45),
                Id = 1
            });
            logWriter.Append(new Checkpoint{
                Timestamp = new DateTime(1100000),
                RiderId = "xxxx",
                Id = 2
            });
            logWriter.Append(new DropCheckpoint{
                Timestamp = new DateTime(1200000),
                Id = 3
            });

            sw.ToString().Should().Be(simpleLog1);
        }
        
        [Fact]
        public void Should_read_logs()
        {
            var sr = new StringReader(simpleLog1);
            var logReader = new LogReader();
            var entries = logReader.Read(sr).ToList();
            
            entries.Count.Should().Be(3);
            entries[0].Should().BeOfType<SessionStart>();
            entries[1].Should().BeOfType<Checkpoint>();
            entries[2].Should().BeOfType<DropCheckpoint>();
            
            entries[0].Timestamp.Should().Be(new DateTime(1000000));
            entries[0].Id.Should().Be(1);
            ((SessionStart)entries[0]).Duration.Should().Be(TimeSpan.FromMinutes(45));
        }
    }
}