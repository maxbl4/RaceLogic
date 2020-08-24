using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Model;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.WsHub
{
    public class AuthControllerTests: IntegrationTestBase
    {
        public AuthControllerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task Get_tokens()
        {
            using var svc = CreateWsHubService();
            var cli = new WsHubManagementClient(svc.ListenUri, TestAdminWsToken);
            var tokens = await cli.GetTokens();
            tokens.Should().Contain(x => x.Token == TestAdminWsToken);
        }
        
        [Fact]
        public async Task Delete_token()
        {
            using var svc = CreateWsHubService();
            var cli = new WsHubManagementClient(svc.ListenUri, TestAdminWsToken);
            await cli.DeleteToken(TestAdminWsToken);
            (await cli.GetTokens()).Should().BeNull();
            cli = new WsHubManagementClient(svc.ListenUri, WsToken1);
            var tokens = await cli.GetTokens();
            tokens.Should().NotContain(x => x.Token == TestAdminWsToken);
        }
        
        [Fact]
        public async Task Upsert_token()
        {
            using var svc = CreateWsHubService();
            var cli = new WsHubManagementClient(svc.ListenUri, TestAdminWsToken);
            await cli.UpsertToken(new AuthToken
            {
                Token = "service-token",
                Roles = "Service",
                ServiceName = "Test Service"
            });
            (await cli.GetTokens()).Should().Contain(x => 
                x.Token == "service-token" && x.Roles == "Service" && x.ServiceName == "Test Service");
            await cli.UpsertToken(new AuthToken
            {
                Token = "admin2",
                Roles = Constants.WsHub.Roles.Admin,
                ServiceName = "Admin 2"
            });
            cli = new WsHubManagementClient(svc.ListenUri, "admin2");
            (await cli.GetTokens()).Should().HaveCount(5);
        }
    }
}