using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Easy.MessageHub;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Rfid
{
    public class RfidServiceTests : StorageServiceFixture
    {
        private RfidService rfidService;
        private readonly FakeSystemClock systemClock;
        private readonly IPEndPoint readerEndpoint = IPEndPoint.Parse("127.0.0.1:0");
        private readonly string serializedConnectionString;
        private readonly SimulatorListener simulator;
        private readonly TagListHandler tagListHandler;

        public RfidServiceTests()
        {
            systemClock = new FakeSystemClock();
            simulator = new SimulatorListener(readerEndpoint);
            tagListHandler = new TagListHandler();
            simulator.TagListHandler = tagListHandler.Handle;
            serializedConnectionString = $"Protocol = Alien; Network = {simulator.ListenEndpoint}";
            storageService.SetRfidSettings(new RfidOptions{RfidEnabled = true, SerializedConnectionString = serializedConnectionString});
        }

        [Fact]
        public async Task Should_persist_checkpoints()
        {
            rfidService = new RfidService(storageService, new MessageHub(), systemClock, new NullLogger<RfidService>());
            var connectionString = storageService.GetRfidSettings().GetConnectionString();
            connectionString.Protocol.ShouldBe(ReaderProtocolType.Alien);
            tagListHandler.ReturnOnce(new Tag{TagId = "1"});
            systemClock.Advance();
            tagListHandler.ReturnOnce(new Tag{TagId = "1"});

            var cps = storageService.ListCheckpoints();
            cps.Count.ShouldBe(2);
        }
    }

    class TagListHandler
    {
        readonly object sync = new object();
        private string returnOnceTags = null;
        TaskCompletionSource<bool> returnTask = null;
        public string Handle()
        {
            lock (sync)
            {
                if (returnOnceTags != null)
                {
                    var t = returnOnceTags;
                    returnOnceTags = null;
                    return t;
                }

                if (returnTask != null)
                {
                    returnTask.TrySetResult(true);
                    returnTask = null;
                }

                return ProtocolMessages.NoTags;
            }
        }

        public void ReturnOnce(params Tag[] tags)
        {
            ReturnOnce((IEnumerable<Tag>)tags);
        }
        
        public void ReturnOnce(IEnumerable<Tag> tags)
        {
            lock (sync)
            {
                returnTask = new TaskCompletionSource<bool>();
                returnOnceTags = string.Join("\r\n", tags.Select(x => x.ToCustomFormatString()));
            }
            returnTask.Task.Wait();
        }
    }
}