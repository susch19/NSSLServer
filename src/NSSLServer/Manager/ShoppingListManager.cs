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
        public static async Task<Result> TransferOwnership(DBContext c, int id, int oldOwner, int newOwner)
        {
            var list = await c.ShoppingLists.Include(l => l.Owner).FirstOrDefaultAsync(x => x.Id == id);
            var owner = list.Owner;

            if(owner.Id != oldOwner )
                return new Result {Error = "You are not the owner from the list" };

            list.Owner = await c.Users.FirstOrDefaultAsync(i => i.Id == newOwner);
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<string> DeleteContributor(DBContext c, int listId, int adminId, int contributorId) // TODO Überhaupt implementieren
        {
            var admin = (await c.Contributors.FirstOrDefaultAsync(x => x.UserId == adminId && x.ListId == listId))?.IsAdmin;
            if (!admin.HasValue || !admin.Value)
                return "insufficient rights";
            c.Contributors.Remove((await c.Contributors.FirstOrDefaultAsync(x => x.UserId == contributorId)));
            await c.SaveChangesAsync();
            return "success";
        }

        public static async Task<Contributor> AddContributor(DBContext c, int listId, int userId, int contributor)
        {
            var admin = (await c.Contributors.FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == listId))?.IsAdmin;
            if (!admin.HasValue || !admin.Value)
                return null;
            var k = await UserManager.FindUserById(c.Connection, contributor);
            var con = new Contributor
            {
                UserId = k.Id,
                IsAdmin = false
            };
            c.Contributors.Add(new Contributor { ListId = listId, UserId = k.Id });
            await c.SaveChangesAsync();
            return con;
        }



        public static async Task<ChangeListItemResult> ChangeProduct(DBContext c, int listId, int contributorId, int productId, int change)
        {
            var shoppinglist = await c.ShoppingLists.Include(x=>x.Contributors).Include(x=>x.Products).FirstOrDefaultAsync(x => x.Id == listId);
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

        public static async Task<bool> ChangeListname(DBContext c, int id, int userId, string newName)
        {
            c.ShoppingLists.FirstOrDefault(x => x.Id == id && x.UserId == userId).Name = newName;
            await c.SaveChangesAsync();
            return true;
        }

        public static async Task<Result> DeleteList(DBContext c, int listId, int userId)
        {
            var shoppinglist = c.ShoppingLists.Include(x=>x.Owner).FirstOrDefault(x => x.Id == listId);
                       
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
            var list = c.ShoppingLists.Include(x=>x.Contributors).Include(x=>x.Products).FirstOrDefault(x => x.Id == listid);
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


        public static async Task<List<ShoppingList>> GetShoppingLists(DBContext con, User user)
        {
            var cont = con.Contributors.Include(x=>x.ShoppingList).Where(x => x.User == user);
            
            var lists = new List<ShoppingList>();
            foreach (var item in cont)
                lists.Add(item.ShoppingList);
            
            return lists;//  con.ShoppingLists.Where(x=>x.Contributors.Contains()).ToList();

            //await con.Entry(user).Collection(m => m.ShoppingLists).LoadAsync();
            //await con.Entry(user).Collection(x => x.IsContributors).LoadAsync();
        }
    }
}
