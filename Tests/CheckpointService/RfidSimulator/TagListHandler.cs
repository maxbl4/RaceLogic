using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Shouldly;

namespace maxbl4.Race.Tests.CheckpointService.RfidSimulator
{
    public class TagListHandler
    {
        readonly object sync = new object();
        private string returnOnceTags = null;
        TaskCompletionSource<bool> returnTask = null;
        private string returnContinuos;

        public string Handle()
        {
            lock (sync)
            {
                LastRequestTime = DateTime.UtcNow;
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

                if (!string.IsNullOrEmpty(returnContinuos))
                    return returnContinuos;

                return ProtocolMessages.NoTags;
            }
        }

        public void ReturnContinuos(params Tag[] tags)
        {
            ReturnContinuos((IEnumerable<Tag>)tags);
        }
        
        public void ReturnContinuos(IEnumerable<Tag> tags)
        {
            lock (sync)
            {
                returnContinuos = string.Join("\r\n", tags.Select(x => x.ToCustomFormatString()));
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

            returnTask.Task.Wait(5000).ShouldBeTrue();
        }
        
        public DateTime LastRequestTime { get; private set; }
        public TimeSpan TimeSinceLastRequest => DateTime.UtcNow - LastRequestTime;
    }
}