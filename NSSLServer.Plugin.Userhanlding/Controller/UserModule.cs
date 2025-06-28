using System.Collections.Generic;
using System.Threading.Tasks;
using static NSSLServer.Shared.RequestClasses;
using static NSSLServer.Shared.ResultClasses;
using Microsoft.AspNetCore.Mvc;
using Deviax.QueryBuilder;
using NSSLServer.Models;
using System.Net;
using NSSLServer.Database.Attributes;
using NSSLServer.Database;
using NSSLServer.Plugin.Userhandling.Manager;

namespace NSSLServer.Plugin.Userhandling.Controller
{
    [Route("users")]
    public class UserModule : AuthenticatingDbContextController
    {
        private List<string> unauthorizedEndpoints = new List<string> { "/users/create", "/users/login" };

        public UserModule()
        {
        }

        [HttpDelete]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> DeleteUser()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        => null;


        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordArgs args)
        {
            if (args.OldPWHash == null || args.NewPWHash == null)
                return Json(new Result { Error = "Username, Old Password or New Password was not correctly inserted" });
            return Json((await UserManager.ChangePassword(Session.Id, args.OldPWHash, args.NewPWHash)));
        }

        [HttpGet, WithDbContext]
        public async Task<IActionResult> Info()
        {
            return Json(await Q.From(Models.User.T).Where(x => x.Id.EqV(Session.Id))
            .Select(u => u.Id,
            u => u.Email.As(N.Db(nameof(InfoResult.EMail))),
            u => u.Username,
            u => Q.From(Contributor.T).Where(x => x.UserId.Eq(u.Id)).Select(a => Q.ArrayAgg(a.ListId).As(N.Db(nameof(InfoResult.ListIds)))))
            .FirstOrDefault<InfoResult>(Context.Connection));
        }


    }
    [Route("registration")]
    public class RegistrationController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]LoginArgs args)
        {
            if (args.Username == null || args.EMail == null || args.PWHash == null)
                return new BadRequestResult();
            return Json((await UserManager.CreateUser(args.Username, args.EMail, args.PWHash)));
        }
    }

   
    [Route("password")]
    public class ResetPassword : BaseController
    {
        //public class Token
        //{
        //    public string token { get; set; }
        //}

        [HttpGet, Route("site/reset")]
        // [Produces("text/html")]
        public ContentResult ResetPasswordWebsite([FromQuery]string token)
        {

            var path = "Static/passwordreset.html";
            var content = System.IO.File.ReadAllText(path);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = content
            };
        }
        public class Email
        {
            public string email { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> RequestResetPasswordEmail([FromBody]Email email)
        {
            if (string.IsNullOrWhiteSpace(email.email))
                return new BadRequestResult();
            return Json(await UserManager.SendPasswortResetEmail(email.email));
            //sender.SendMail("suschi@ist-einmalig.de", "Test Mail", "Hello!");

            //return new BadRequestResult();
        }
        public class TokenPW
        {
            public string token { get; set; }
            public string password { get; set; }
        }

        [HttpPut, Route("reset")]
        public async Task<IActionResult> RequestResetPassword([FromBody]TokenPW token)
        {
            return Json(await UserManager.ResetPassword(token.token, token.password));
        }

        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordArgs args)
        {
            if (args.OldPWHash == null || args.NewPWHash == null)
                return Json(new Result { Error = "Username, Old Password or New Password was not correctly inserted" });
            return Json((await UserManager.ChangePassword(Session.Id, args.OldPWHash, args.NewPWHash)));
        }
    }

}
