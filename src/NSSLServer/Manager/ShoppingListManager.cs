using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSSLServer.Sources;
using NSSLServer.Models;
using Deviax.QueryBuilder;
using NSSLServer.Models.DatabaseConnection;
using System.Threading;
using NSSLServer.Module;
using Deviax.QueryBuilder.Parts;
using static Shared.RequestClasses;
using static Shared.ResultClasses;
using Microsoft.EntityFrameworkCore;

namespace NSSLServer
{
    //public static class AddContributorRoute
    //{
    //    public static readonly AuthenticatedRoute R = Execute;
    //    private class Args
    //    {
    //        public int? ListId { get; set; }
    //        public int? ContributorId { get; set; }
    //    }

    //    public class Result
    //    {
    //        public bool Success;
    //        public string Error;
    //        // public int? ContributorId;
    //    }

    //    public static async Task<dynamic> Execute(NancyContext ctx, int userId, DBContext c)
    //    {

    //        var args = ctx.Request.AsJson<Args>();

    //        if (!args.ListId.HasValue || !args.ContributorId.HasValue)
    //            throw new ArgumentNullException();

    //        //return await ShoppingListManager.AddContributor(context, args.ListId.Value, UserManager.GetIdFromToken(Context.Request.Query["token"]), args.ContributorId.Value);
    //        //    var userId = (int)stuff.userId;
    //        var admin = (await c.Contributors.FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == args.ListId))?.IsAdmin;
    //        if (!admin.HasValue || !admin.Value)
    //            return new Result { Error = "Nope" };

    //        var k = await UserManager.FindUserById(c.Database.Connection, args.ContributorId.Value);
    //        c.ShoppingLists.FirstOrDefault(x => x.Id == args.ListId).Contributors.Add(new Contributor
    //        {
    //            UserId = k.Id,
    //            IsAdmin = false
    //        });
    //        await c.SaveChangesAsync();

    //        return new Result
    //        {
    //            Success = true
    //        };
    //    }
    //}

    public static class ShoppingListManager
    {

        public static async Task<ShoppingList> LoadShoppingList(int listId, int userId)
        {
            //DBContext co;
            //var cont = (await c.Contributors.FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == listId));
            //var cont2 = (await c.Contributors.Include(a => a.ShoppingList).FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == listId));
            //var u = (await co.Contributors.Include(a => a.ShoppingList).FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == listId))?.ShoppingList;
            //var sl = cont?.ShoppingList;
            //var sl2 = cont2?.ShoppingList;


            //  c.Contributors.Where(c => c.UserId == userId)

            //var list = (await c.ShoppingLists.FirstOrDefaultAsync(x => x.Id == listId));
            //var cont = list.Contributors.FirstOrDefault(x => x.Id == userId);

            using (var con = await NsslEnvironment.OpenConnectionAsync())
            {

                var list = await Q.From(Contributor.CT)
                    .InnerJoin(ShoppingList.SLT).On((c, sl) => c.ListId.Eq(sl.Id))
                    .Where(
                        (c, sl) => c.UserId.Eq(userId),
                        (c, sl) => sl.Id.Eq(listId)
                    )
                    .Select(new RawSql(ShoppingList.SLT.TableAlias + ".*")).Limit(1)
                    .FirstOrDefault<ShoppingList>(con);



                if (list == null)
                    return new ShoppingList { }; //TODO Nicht leere Liste zurückgeben
                list.Products = await Q.From(ListItem.LIT).Where(l => l.ListId.Eq(list.Id))
                        .Where(l => l.Amount.Neq(Q.P("a", 0)))
                        .OrderBy(t => t.Id.Asc())
                        .ToList<ListItem>(con);
                return list;
            }
        }


        public static async Task<Result> ChangeRights(DBContext c, int id, int requesterId, int changeUserId)
        {
            var list = await c.ShoppingLists.Include(x => x.Owner).Include(x => x.Contributors).FirstOrDefaultAsync(x => x.Id == id);
            var requester = list.Contributors.FirstOrDefault(x => x.Id == requesterId);

            if (requester.IsAdmin && changeUserId == list.Owner.Id)
                return new Result { Error = "You are not an admin or you wanted to demote the owner" };

            var changeUser = list.Contributors.FirstOrDefault(x => x.Id == changeUserId);
            changeUser.IsAdmin = !changeUser.IsAdmin;
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<Result> DeleteContributor(DBContext c, int listId, int adminId, int contributorId)
        {
            var admin = (await c.Contributors.FirstOrDefaultAsync(x => x.UserId == adminId && x.ListId == listId))?.IsAdmin;
            if (!admin.HasValue || !admin.Value)
                return new Result { Success = false, Error = "insufficient rights" };
            c.Contributors.Remove((await c.Contributors.FirstOrDefaultAsync(x => x.UserId == contributorId)));
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<AddContributorResult> AddContributor(DBContext c, int listId, int userId, User contributor)
        {
            var admin = (await c.Contributors.FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == listId))?.IsAdmin;

            if (!admin.HasValue || !admin.Value)
                return new AddContributorResult { Success = false, Error = "insufficient rights" };
            var existingContributor = await c.Contributors.FirstOrDefaultAsync(x => x.UserId == contributor.Id && x.ListId == listId);
            if (existingContributor != null)
                return new AddContributorResult { Success = false, Error = "user is already contributor" };
            var con = new Contributor
            {
                UserId = contributor.Id,
                IsAdmin = false
            };
            c.Contributors.Add(new Contributor { ListId = listId, UserId = contributor.Id });
            await c.SaveChangesAsync();

            return new AddContributorResult { Success = true, Id = contributor.Id, Name = contributor.Username };
        }

        public static async Task<GetContributorsResult> GetContributors(DBContext c, int listId, int userId)
        {

            var cont = c.ShoppingLists.Include(x => x.Contributors).FirstOrDefault(x => x.Id == listId);
            var contributors = cont.Contributors;
            using (var con = await NsslEnvironment.OpenConnectionAsync())
                foreach (var item in contributors)
                    item.User = await Q.From(User.UT).Where(x => x.Id.Eq(item.UserId)).FirstOrDefault<User>(con);
            if (contributors.FirstOrDefault(x => x.UserId == userId) == null)
                return new GetContributorsResult { Success = false, Error = "User is part of the list" };
            return new GetContributorsResult
            {
                Success = true,
                Contributors = contributors.
                    Select(x => new GetContributorsResult.ContributorResult
                    { IsAdmin = x.IsAdmin, Name = x.User.Username, UserId = x.UserId }).
                    ToList()
            };
        }

        public static async Task<ChangeListItemResult> ChangeProduct(DBContext c, int listId, int contributorId, int productId, int change)
        {
            var shoppinglist = await c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == listId);
            if (shoppinglist.Contributors.FirstOrDefault(x => x.UserId == contributorId) == null)
                return new ChangeListItemResult { Success = false, Error = "User is not allowed to access this list" };
            var product = shoppinglist.Products.FirstOrDefault(x => x.Id == productId);
            if (product == null)
                return new ChangeListItemResult { Success = false, Error = "Product not found" };
            if (product.Amount + change <= 0 || change == 0)
                product.Amount = 0;
            else
                product.Amount += change;
            await c.SaveChangesAsync();
            return new ChangeListItemResult { Success = true, Id = product.Id, Name = product.Name, Amount = product.Amount, ListId = listId };
        }

        public static async Task<Result> ChangeProducts(DBContext c, int listId, int contributorId, List<int> productIds, List<int> changes)
        {
            var shoppinglist = await c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == listId);
            if (shoppinglist.Contributors.FirstOrDefault(x => x.UserId == contributorId) == null)
                return new Result { Success = false, Error = "User is not allowed to access this list" };
            if (productIds.Count != changes.Count)
                return new Result { Success = false, Error = "Length of product ids doesn't match with length of change list" };
            var notFoundIds = new List<int>();
            int hash = 0;

            for (int i = 0; i < productIds.Count; i++)
            {
                var id = productIds[i];
                var change = changes[i];
                var product = shoppinglist.Products.FirstOrDefault(x => x.Id == id);
                if (product == null)
                    notFoundIds.Add(id);
                else
                {
                    if (product.Amount + change <= 0 || change == 0)
                        product.Amount = 0;
                    else
                        product.Amount += change;
                    hash += product.Amount + product.Id;
                }
            }
            await c.SaveChangesAsync();


            if (notFoundIds.Count == 0)
                return new HashResult { Success = true, Hash = hash};
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }

        public static async Task<ChangeListNameResult> ChangeListname(DBContext c, int id, int userId, string newName)
        {
            var list = c.ShoppingLists.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            if (list.Contributors.FirstOrDefault(x => x.IsAdmin == true && x.User.Id == userId) == null)
                return new ChangeListNameResult { Success = false, Error = "Insufficient rights" };

            list.Name = newName;
            await c.SaveChangesAsync();
            return new ChangeListNameResult { Success = true, ListId = list.Id, Name = list.Name };
        }

        public static async Task<Result> DeleteList(DBContext c, int listId, int userId)
        {
            var shoppinglist = c.ShoppingLists.Include(x => x.Owner).FirstOrDefault(x => x.Id == listId);

            if (shoppinglist == null)
                return new Result { Success = false, Error = "The List could not be found. Maybe it was deleted already." };
            if (shoppinglist.Owner.Id != userId)
                return new Result { Success = false, Error = "Only owner is able to delete the list" };
            c.ShoppingLists.Remove(shoppinglist);
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<AddListResult> AddList(DBContext c, string listName, int userId)
        {
            var user = await c.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new AddListResult { Success = false, Error = "User not found" };
            //var user = await UserManager.FindUserById(c.Database.Connection, userId);
            var shoppinglist = c.ShoppingLists.Add(new ShoppingList { Name = listName, UserId = user.Id, Owner = user }).Entity;

            c.Contributors.Add(new Contributor { IsAdmin = true, ShoppingList = shoppinglist, UserId = user.Id });

            //user.IsContributors.Add();

            //z.Contributors.Add(user.IsContributors.FirstOrDefault(x => x.ListId == z.Id));
            //c.Users.Attach(user);
            await c.SaveChangesAsync();
            return new AddListResult { Success = true, Id = shoppinglist.Id, Name = shoppinglist.Name };
        }

        public static async Task<Result> DeleteProduct(DBContext c, int listid, int userid, int productid)
        {
            var list = c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefault(x => x.Id == listid);
            if (list == null)
                return new Result { Success = false, Error = "list could not be found" };
            var user = list.Contributors.FirstOrDefault(x => x.UserId == userid);
            if (user == null)
                return new Result { Success = false, Error = "You are not a contributor" };
            var p = list.Products.FirstOrDefault(x => x.Id == productid);
            if (p == null)
                return new Result { Success = false, Error = "Product could not be found in the database" };
            p.Amount = 0;
            //list.Products.Remove(list.Products.FirstOrDefault(x => x.Id == productid));
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<Result> DeleteProducts(DBContext c, int listid, int userid, List<int> productIds)
        {
            var list = c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefault(x => x.Id == listid);
            if (list == null)
                return new Result { Success = false, Error = "list could not be found" };
            var user = list.Contributors.FirstOrDefault(x => x.UserId == userid);
            if (user == null)
                return new Result { Success = false, Error = "You are not a contributor" };
            var notFoundIds = new List<int>();

            foreach (var id in productIds)
            {
                var p = list.Products.FirstOrDefault(x => x.Id == id);
                if (p == null)
                    notFoundIds.Add(id);
                else
                    p.Amount = 0;
            }
            await c.SaveChangesAsync();

            if (notFoundIds.Count == 0)
                return new Result { Success = true };
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }

        public static async Task<AddListItemResult> AddProduct(DBContext c, int listid, int userid, string name, string gtin, int amount)
        {
            var list = c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefault(x => x.Id == listid);
            if (list == null)
                return new AddListItemResult { Success = false, Error = "list could not be found" };
            var user = list.Contributors.FirstOrDefault(x => x.UserId == userid);
            if (user == null)
                return new AddListItemResult { Success = false, Error = "You are not a contributor" };
            var li = new ListItem { Gtin = gtin, Name = name, Amount = amount, ListId = listid };
            list.Products.Add(li);
            await c.SaveChangesAsync();
            //TODO Same name and same gtin
            return new AddListItemResult { Success = true, Gtin = li.Gtin, Name = name, ProductId = li.Id };
        }

        //private static async Task<ShoppingList> FindListById(int id) =>
        //    await Q.From(ShoppingList.SLT).Where(ShoppingList.SLT.Id.Eq(Q.P("id", id))).FirstOrDefault<ShoppingList>(await DBConnection.OpenConnection());


        public static async Task<List<ShoppingList>> GetShoppingLists(DBContext con, int userId)
        {
            var cont = con.Contributors.Include(x => x.ShoppingList).Where(x => x.User.Id == userId);

            var lists = new List<ShoppingList>();
            foreach (var item in cont)
                lists.Add(item.ShoppingList);

            return lists;//  con.ShoppingLists.Where(x=>x.Contributors.Contains()).ToList();

            //await con.Entry(user).Collection(m => m.ShoppingLists).LoadAsync();
            //await con.Entry(user).Collection(x => x.IsContributors).LoadAsync();
        }

        internal static async Task<ListsResult> LoadShoppingLists(DBContext con, int userId)
        {
            var lists = await GetShoppingLists(con, userId);

            var dic = lists.Select(y => new ListsResult.ListResultItem { Id = y.Id, Name = y.Name, IsAdmin = y.Contributors.FirstOrDefault(z => z.UserId == userId).IsAdmin }).ToList();

            return new ListsResult { Lists = dic };
        }
    }
}
