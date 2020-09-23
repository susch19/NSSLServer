using Deviax.QueryBuilder;
using Microsoft.AspNetCore.Mvc;

using NSSLServer.Plugin.Userhandling.Manager;
using NSSLServer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Shared.RequestClasses;
using static Shared.ResultClasses;
namespace NSSLServer.Plugin.Userhandling.Controller
{
    [Route("session")]
    public class SessionController : BaseController
    {

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginArgs args)
        {
            if ((args?.Username == null && args?.EMail == null) || args?.PWHash == null)
                return new BadRequestResult();
            return Json(await UserManager.Login(args.Username?.Trim()?.ToLower(), args.EMail?.Trim().ToLower(), args.PWHash));
        }

        public class RefreshResult
        {
            public string Token;
            public NsslSession Session;

        }

        [HttpPut]
        [AuthRequired]
        public Task<IActionResult> Refresh()
        {
            var payload = new Dictionary<string, object>()
                {
                    { "Expires", DateTime.UtcNow.AddMonths(1) },
                    { "Id", Session.Id},
                    {"Created", DateTime.UtcNow }
                };
            
            //var token = JsonWebToken.Encode(new Dictionary<string, object>(), Session, ExtractJwtSessionFilterAttribute.JwtKeyBytes, JsonWebToken.JwtHashAlgorithm.HS512);
            var token =  JsonWebToken.Encode(new Dictionary<string, object>(), payload, ExtractJwtSessionFilterAttribute.JwtKeyBytes, JsonWebToken.JwtHashAlgorithm.HS256);
            return Task.FromResult<IActionResult>(Json(new SessionRefreshResult { Token = token }));
        }

        [HttpGet]
        [AuthRequired]
        public Task<IActionResult> Get() => Task.FromResult<IActionResult>(Json(Session));
    }


}
