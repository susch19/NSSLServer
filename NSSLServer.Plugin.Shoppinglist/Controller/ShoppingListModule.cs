using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSSLServer.Database;
using NSSLServer.Database.Attributes;
using NSSLServer.Models;
using NSSLServer.Plugin.Shoppinglist.Manager;
using NSSLServer.Plugin.Userhandling.Manager;
using System.Threading.Tasks;

using static NSSLServer.Shared.RequestClasses;
using static NSSLServer.Shared.ResultClasses;

namespace NSSLServer.Plugin.Shoppinglist.Controller
{
    [Route("shoppinglists"), WithDbContext, ExtractDeviceToken]
    public class ShoppingListModule : AuthenticatingDbContextController
    {
        //[HttpGet, Route("products/{identifier}")]
        //public async Task<IActionResult> GetProduct(string identifier)
        //{
        //    if (string.IsNullOrWhiteSpace(identifier))
        //        return new BadRequestResult();
        //    if ((identifier.Length == 8 || identifier.Length == 13) && long.TryParse(identifier, out long i))
        //        return Json((await ProductSourceManager.FindProductByCode(identifier)));
        //    else
        //        return Json((await ProductSourceManager.FindProductsByName(identifier)));
        //}

        [HttpGet]
        [Route("{listId}/{alreadyBought}")]
        public async Task<IActionResult> GetList(int listId, bool alreadyBought)
            => listId != 0 ? (IActionResult)(Json(await ShoppingListManager.LoadShoppingList(listId, alreadyBought, Session.Id))) : new BadRequestResult();

        [HttpGet]
        [Route("batchaction/"), Route("")]
        public async Task<IActionResult> GetLists()
        {
            return Json(await ShoppingListManager.LoadShoppingLists(Context, Session.Id));
        }

        [HttpPut]
        [Route("{listId}/contributors/{contributorId}")]
        public async Task<IActionResult> ChangeRights(int listId, int contributorId)
        {
            if (listId == 0 || contributorId == 0)
                return new BadRequestResult();
            return Json(await ShoppingListManager.ChangeRights(Context, listId, Session.Id, contributorId));
        }

        [HttpDelete]
        [Route("{listId}/contributors/{contributorId}")]
        public async Task<IActionResult> DeleteContributor(int listId, int contributorId)
        {
            if (listId == 0 || contributorId == 0)
                return new BadRequestResult();
            return Json(await ShoppingListManager.DeleteContributor(Context, listId, Session.Id, contributorId));

        }

        [HttpPost, Route("{listId}/contributors")]
        public async Task<IActionResult> AddContributor(int listId, [FromBody] AddContributorArgs args)
        {
            if (listId == 0 || string.IsNullOrWhiteSpace(args.Name))
                return Json(new AddContributorResult { Error = "Wrong arguments" });// new Response { StatusCode = HttpStatusCode.BadRequest };

            User u = await UserManager.FindUserByName(Context.Connection, args.Name);
            ;

            if (u == null)
                return Json(new AddContributorResult { Error = "User not found" });

            return Json(await ShoppingListManager.AddContributor(Context, listId, Session.Id, u, HttpContext.GetDeviceToken()));
        }

        [HttpGet, Route("{listId}/contributors")]
        public async Task<IActionResult> GetContributors(int listId)
        {
            if (listId == 0)
                return Json(new GetContributorsResult { Error = "The list id was not provided", Success = false });
            return Json(await ShoppingListManager.GetContributors(Context, listId, Session.Id));
        }

        [HttpPut, Route("{listId}/products/{productId}")]
        public async Task<IActionResult> ChangeProduct(int listId, int productId, [FromBody] ChangeProductArgs args)
        {
            if (listId == 0 || productId == 0 || (!args.Change.HasValue && string.IsNullOrWhiteSpace(args.NewName)))
                return new BadRequestResult();
            return Json((await ShoppingListManager.ChangeProduct(Context, listId, Session.Id, productId, args.Change.HasValue ? args.Change.Value : 0, args.Order, args.NewName, HttpContext.GetDeviceToken())));
        }

        [HttpPut, Route("{listId}")]
        public async Task<IActionResult> RenameList(int listId, [FromBody] ChangeListNameArgs args)
        {
            if (listId == 0 || string.IsNullOrWhiteSpace(args.Name))
                return Json(new Result { Success = false, Error = "ListID is wrong or name is empty" });
            return Json(await ShoppingListManager.ChangeListname(Context, listId, Session.Id, args.Name, HttpContext.GetDeviceToken()));
        }

        [HttpDelete, Route("{listId}")]
        public async Task<IActionResult> DeleteList(int listId)
        {
            if (listId == 0)
                return Json(new Result { Error = "No list id was provided", Success = false });
            return Json((await ShoppingListManager.DeleteList(Context, listId, Session.Id, HttpContext.GetDeviceToken())));
        }

        [HttpPost]
        public async Task<IActionResult> AddList([FromBody] AddListArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Name))
                return new BadRequestResult();

            return Json((await ShoppingListManager.AddList(Context, args.Name, Session.Id)));
        }


        [HttpDelete, Route("{listId}/products/{productId}")]
        public async Task<IActionResult> DeleteProduct(int listId, int productId)
        {
            if (listId == 0 || productId == 0)
                return new BadRequestResult();
            return Json(await ShoppingListManager.DeleteProduct(Context, listId, Session.Id, productId, HttpContext.GetDeviceToken()));
        }

        [HttpPost, Route("{listId}/products/batchaction/{command}")]
        public async Task<IActionResult> BatchAction(int listId, string command, [FromBody] BatchProductArgs args)
        {
            if (listId == 0 || args.ProductIds.Count == 0 || string.IsNullOrWhiteSpace(command))
                return new BadRequestResult();
            switch (command.ToLower())
            {
                case "delete":
                    return Json(await ShoppingListManager.DeleteProducts(Context, listId, Session.Id, args.ProductIds, HttpContext.GetDeviceToken()));
                case "change":
                    return Json(await ShoppingListManager.ChangeProducts(Context, listId, Session.Id, args.ProductIds, args.Amount, HttpContext.GetDeviceToken()));
                case "order":
                    return Json(await ShoppingListManager.ReorderProducts(Context, listId, Session.Id, args.ProductIds, HttpContext.GetDeviceToken()));
                default:
                    return Json(new Result { Error = "action could not be found", Success = false });
            }
        }

        [HttpPost, Route("{listId}/products")]
        public async Task<IActionResult> AddProduct(int listId, [FromBody] AddProductArgs args)
        {
            Plugin.Logger.LogWarning("""Adding product with name "{productName}" and gtin "{gtin}" to list {listId}""", args.ProductName, args.Gtin, listId);
            if (listId == 0
                || ((string.IsNullOrWhiteSpace(args.Gtin) 
                        || !ulong.TryParse(args.Gtin, out _)) 
                    && string.IsNullOrWhiteSpace(args.ProductName)
                    ))
                return new BadRequestResult();

            return Json((await ShoppingListManager.AddProduct(Context, listId, Session.Id, args.ProductName, args.Gtin, args.Amount.Value, args.Order, HttpContext.GetDeviceToken())));
        }
    }
}
