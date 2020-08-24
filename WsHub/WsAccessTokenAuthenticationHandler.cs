using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using maxbl4.Race.WsHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace maxbl4.Race.WsHub
{
    public class WsAccessTokenAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IAuthService authService;
        
        public WsAccessTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock, IAuthService authService) : base(options, logger, encoder, clock)
        {
            this.authService = authService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"],
                out AuthenticationHeaderValue headerValue))
                return Task.FromResult(AuthenticateResult.Fail("Missing or bad authorization header"));
            if (string.IsNullOrWhiteSpace(headerValue.Parameter))
                return Task.FromResult(AuthenticateResult.Fail("Empty user id"));

            return Task.FromResult(authService.ValidateToken(headerValue.Parameter));
        }
    }
}