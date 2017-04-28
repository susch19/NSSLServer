using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using static Shared.RequestClasses;
using static Shared.ResultClasses;
using Microsoft.AspNetCore.Mvc;

namespace NSSLServer.Module
{
    [Route("users")]
    public class UserModule : AuthenticatingController 
    {
        private List<string> unauthorizedEndpoints = new List<string> { "/users/create", "/users/login" };
        
        public UserModule()
        {
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        => null;
        
        
        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordArgs args)
        {
            if (args.OldPWHash == null || args.NewPWHash == null)
                return Json(new Result { Error = "Username, Old Password or New Password was not correctly inserted" });
            return Json((await UserManager.ChangePassword(Session.Id,args.OldPWHash,args.NewPWHash)));
        }

        [HttpGet, WithDbContext]
        public async Task<IActionResult> Info()
        {
            var u = await UserManager.FindUserById(Context.Connection, Session.Id);
            var lists = await ShoppingListManager.GetShoppingLists(Context, u.Id);
            var re = new InfoResult {Id = u.Id, EMail = u.Email, Username = u.Username };
            re.ListIds = new List<int>();
            lists.ForEach(y => re.ListIds.Add(y.Id));
            return Json(re);
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
            return Json((await UserManager.CreateUser(args.Username.ToLower(), args.EMail.ToLower(), args.PWHash)));
        }
    }
}
