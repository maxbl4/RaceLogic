using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.WsHub.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace maxbl4.Race.WsHub.Services
{
    public interface IAuthService
    {
        AuthenticateResult ValidateToken(string token);
        IEnumerable<AuthToken> GetTokens();
        bool UpsertToken(AuthToken token);
        bool DeleteToken(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly WsHubRepository wsHubRepository;
        private readonly ConcurrentDictionary<string, AuthToken> tokenCache;

        public AuthService(WsHubRepository wsHubRepository, IOptions<ServiceOptions> options)
        {
            this.wsHubRepository = wsHubRepository;
            var tokens = wsHubRepository.GetTokens()
                .GroupBy(x => x.Token)
                .Select(x => new KeyValuePair<string, AuthToken>(x.Key, x.First()));
            tokenCache = new ConcurrentDictionary<string, AuthToken>(tokens);

            if (tokenCache.Count == 0)
            {
                var initialTokens = options.Value.InitialAdminTokens?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (initialTokens != null)
                    foreach (var initialToken in initialTokens)
                        UpsertToken(new AuthToken
                            {Token = initialToken, ServiceName = initialToken, Roles = Constants.WsHub.Roles.Admin});
            }
        }

        public IEnumerable<AuthToken> GetTokens()
        {
            return tokenCache.Values.OrderBy(x => x.ServiceName);
        }

        public bool UpsertToken(AuthToken token)
        {
            var b = wsHubRepository.UpsertToken(token);
            tokenCache[token.Token] = token;
            return b;
        }

        public bool DeleteToken(string token)
        {
            var b = wsHubRepository.DeleteToken(token);
            tokenCache.TryRemove(token, out _);
            return b;
        }

        public AuthenticateResult ValidateToken(string token)
        {
            if (!tokenCache.TryGetValue(token, out var auth))
                return AuthenticateResult.Fail("Not found");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, auth.ServiceName)
            };
            var roles = auth.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (roles != null)
                claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
            var identity = new ClaimsIdentity(claims, Constants.WsHub.Authentication.SchemeName);
            var principal = new ClaimsPrincipal(identity);

            return AuthenticateResult.Success(new AuthenticationTicket(principal,
                Constants.WsHub.Authentication.SchemeName));
        }
    }
}