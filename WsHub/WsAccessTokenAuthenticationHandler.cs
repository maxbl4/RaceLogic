using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace maxbl4.Race.WsHub
{
    public class WsAccessTokenAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "WsAccessToken";
        public WsAccessTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"],
                out AuthenticationHeaderValue headerValue))
                return Task.FromResult(AuthenticateResult.Fail("Missing or bad authorization header"));
            if (string.IsNullOrWhiteSpace(headerValue.Parameter))
                return Task.FromResult(AuthenticateResult.Fail("Empty user id"));
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, headerValue.Parameter)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}