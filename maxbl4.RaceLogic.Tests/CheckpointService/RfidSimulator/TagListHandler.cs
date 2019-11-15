using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;

namespace maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator
{
    public class TagListHandler
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
                returnOnceTags = string.Join("\r\n", tags.Select(x => TagParser.ToCustomFormatString(x)));
            }

            returnTask.Task.Wait(5000);
        }
    }
}