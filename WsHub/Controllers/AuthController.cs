using System.Collections.Generic;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.WsHub.Model;
using maxbl4.Race.WsHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.WsHub.Controllers
{
    [ApiController]
    [Authorize(Roles = Constants.WsHub.Roles.Admin)]
    [Route("tokens")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpGet]
        public IEnumerable<AuthToken> Get()
        {
            return authService.GetTokens();
        }

        [HttpPut]
        [HttpPost]
        public bool Post(AuthToken token)
        {
            return authService.UpsertToken(token);
        }

        [HttpDelete("{token}")]
        public bool Delete(string token)
        {
            return authService.DeleteToken(token);
        }
    }
}