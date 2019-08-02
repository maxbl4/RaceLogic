using System;
using System.IO;
using System.Linq;
using maxbl4.RaceLogic.LogManagement.EntryTypes;
using maxbl4.RaceLogic.LogManagement.IO;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.LogManagement
{
    public class LogWriterTests
    {
        private readonly string simpleLog1 =
            @"{'$type':'start','Duration':'00:45:00','Timestamp':'0001-01-01T00:00:00.1','Sequence':1}
{'$type':'rfid','RiderId':'xxxx','Timestamp':'0001-01-01T00:00:00.11','Sequence':2}
{'$type':'manual','RiderId':'123','Timestamp':'0001-01-01T00:00:00.12','Sequence':3}
".Replace('\'', '"');
        
        [Fact]
        public void Should_write_logs()
        {
            var sw = new StringWriter();
            
            var logWriter = new LogWriter(sw);
            logWriter.Append(new SessionStart {
                Timestamp = new DateTime(1000000),
                Duration = TimeSpan.FromMinutes(45),
                Sequence = 1
            });
            logWriter.Append(new RfidCheckpoint{
                Timestamp = new DateTime(1100000),
                RiderId = "xxxx",
                Sequence = 2
            });
            logWriter.Append(new ManualCheckpoint{
                Timestamp = new DateTime(1200000),
                RiderId = "123",
                Sequence = 3
            });

            sw.ToString().ShouldBe(simpleLog1);
        }
        
        [Fact]
        public void Should_read_logs()
        {
            var sr = new StringReader(simpleLog1);
            var logReader = new LogReader();
            var entries = logReader.Read(sr).ToList();
            
            entries.Count.ShouldBe(3);
            entries[0].ShouldBeOfType<SessionStart>();
            entries[1].ShouldBeOfType<RfidCheckpoint>();
            entries[2].ShouldBeOfType<ManualCheckpoint>();
            
            entries[0].Timestamp.ShouldBe(new DateTime(1000000));
            entries[0].Sequence.ShouldBe(1);
            ((SessionStart)entries[0]).Duration.ShouldBe(TimeSpan.FromMinutes(45));
        }
    }
}