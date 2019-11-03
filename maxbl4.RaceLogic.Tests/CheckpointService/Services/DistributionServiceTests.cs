using System;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Services
{
    public class DistributionServiceTests : StorageServiceFixture
    {
        [Fact]
        public void Should_send_to_other_clients_when_one_fails()
        {
            // One client may throw in OnNext, but observable should not fail and continue sending
            throw new NotImplementedException();
        }
    }
}