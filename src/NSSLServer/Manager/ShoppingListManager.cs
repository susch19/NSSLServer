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
    public static class ShoppingListManager
    {
        public static async Task<ShoppingList> LoadShoppingList(int listId, int userId)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
            {
                var list = await Q.From(Contributor.CT)
                    .InnerJoin(ShoppingList.SLT).On((c, sl) => c.ListId.Eq(sl.Id))
                    .Where(
                        (c, sl) => c.UserId.EqV(userId),
                        (c, sl) => sl.Id.EqV(listId)
                    )
                    .Select(new RawSql(ShoppingList.SLT.TableAlias + ".*")).Limit(1)
                    .FirstOrDefault<ShoppingList>(con);

                if (list == null)
                    return new ShoppingList { }; //TODO Nicht leere Liste zurückgeben
                list.Products = await Q.From(ListItem.LIT).Where(l => l.ListId.EqV(list.Id))
                        .Where(l => l.Amount.Neq(Q.P("a", 0)))
                        .OrderBy(t => t.Id.Asc())
                        .ToList<ListItem>(con);
                return list;
            }
        }


        public static async Task<Result> ChangeRights(DBContext c, int id, int requesterId, int changeUserId)
        {
            var list = await c.ShoppingLists.Include(x => x.Owner).Include(x => x.Contributors).FirstOrDefaultAsync(x => x.Id == id);
            var requester = list.Contributors.FirstOrDefault(x => x.UserId == requesterId);

            if (!requester.IsAdmin || changeUserId == list.Owner.Id)
                return new Result { Error = "You are not an admin or you wanted to demote the owner" };

            var changeUser = list.Contributors.FirstOrDefault(x => x.UserId == changeUserId);
            if (changeUser == null)
                return new Result { Error = "User could not be found as a contributor in this list" };

            changeUser.IsAdmin = !changeUser.IsAdmin;
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<Result> DeleteContributor(DBContext c, int listId, int adminId, int contributorId)
        {
            var list = await c.ShoppingLists.Include(x => x.Owner).Include(x => x.Contributors).FirstOrDefaultAsync(x => x.Id == listId);
            var admin = list.Contributors.FirstOrDefault(x => x.UserId == adminId)?.IsAdmin;
            if (!admin.HasValue || !admin.Value)
                return new Result { Success = false, Error = "insufficient rights" };
            if (contributorId == list.Owner.Id)
                return new Result { Success = false, Error = "owner of a list can't be removed" };
            c.Contributors.Remove((await c.Contributors.FirstOrDefaultAsync(x => x.UserId == contributorId)));
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<AddContributorResult> AddContributor(DBContext c, int listId, int userId, User contributor)
        {
            var cont = await c.Contributors.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId && x.ListId == listId);
            var admin = cont?.IsAdmin;

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

            ShoppingList list;
            List<ListItem> items;
            using (var connection = await NsslEnvironment.OpenConnectionAsync())
            {
                list = await Q.From(ShoppingList.SLT)
                      .Where(sl => sl.Id.EqV(listId)).FirstOrDefault<ShoppingList>(connection);
                items = await Q.From(ListItem.LIT).Where(li => li.ListId.EqV(listId), li => li.Amount.NeqV(0)).ToList<ListItem>(connection);
            }

            await FirebaseCloudMessaging.fcm.TopicMessage<object>(contributor.Username + "userTopic", null,
                     notification: new Firebase.Models.Notification { Title = "You were added to the list " + list.Name },
                     priority: Firebase.Priority.normal);
            await FirebaseCloudMessaging.fcm.TopicMessage(contributor.Username + "userTopic",
                    new { listId, list.Name, items },
                    priority: Firebase.Priority.normal);

            await c.SaveChangesAsync();
            return new AddContributorResult { Success = true, Id = contributor.Id, Name = contributor.Username };
        }

        public static async Task<GetContributorsResult> GetContributors(DBContext c, int listId, int userId)
        {

            var cont = c.ShoppingLists.Include(x => x.Contributors).FirstOrDefault(x => x.Id == listId);
            var contributors = cont.Contributors;
            using (var con = await NsslEnvironment.OpenConnectionAsync())
                foreach (var item in contributors)
                    item.User = await Q.From(User.UT).Where(x => x.Id.EqV(item.UserId)).FirstOrDefault<User>(con);
            if (contributors.FirstOrDefault(x => x.UserId == userId) == null)
                return new GetContributorsResult { Success = false, Error = "User is not part of the list" };
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
            string action;
            if (product.Amount + change <= 0 || change == 0)
            {
                product.Amount = 0;
                action = "ItemDeleted";
                await FirebaseCloudMessaging.fcm.TopicMessage(shoppinglist.Id + "shoppingListTopic",
                                        new { listId, product.Id, action },
                                        priority: Firebase.Priority.normal);
            }
            else
            {
                product.Amount += change;
                if (product.Amount - change == 0)
                {
                    action = "NewItemAdded";
                    await FirebaseCloudMessaging.fcm.TopicMessage(shoppinglist.Id + "shoppingListTopic",
                            new { listId, product.Id, product.Amount, product.Name, action },
                            priority: Firebase.Priority.normal);
                }
                else
                {
                    action = "ItemChanged";
                    await FirebaseCloudMessaging.fcm.TopicMessage(shoppinglist.Id + "shoppingListTopic",
                            new { listId, product.Id, product.Amount, action },
                            priority: Firebase.Priority.normal);
                }

            }
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
            string action = "Refresh";
            await FirebaseCloudMessaging.fcm.TopicMessage(shoppinglist.Id + "shoppingListTopic",
                    new { listId, action },
                    priority: Firebase.Priority.normal);

            await c.SaveChangesAsync();

            if (notFoundIds.Count == 0)
                return new HashResult { Success = true, Hash = hash };
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }

        public static async Task<ChangeListNameResult> ChangeListname(DBContext c, int id, int userId, string newName)
        {
            var list = c.ShoppingLists.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            if (list.Contributors.FirstOrDefault(x => x.IsAdmin == true && x.User.Id == userId) == null)
                return new ChangeListNameResult { Success = false, Error = "Insufficient rights" };

            list.Name = newName;
            var listId = list.Id;

            string action = "ListRename";
            await FirebaseCloudMessaging.fcm.TopicMessage(list.Id + "shoppingListTopic",
                    new { listId, list.Name, action },
                    priority: Firebase.Priority.normal);

            await c.SaveChangesAsync();
            return new ChangeListNameResult { Success = true, ListId = list.Id, Name = list.Name };
        }

        public static async Task<Result> DeleteList(DBContext c, int listId, int userId)
        {
            var shoppinglist = c.ShoppingLists.Include(x => x.Owner).Include(x => x.Contributors).FirstOrDefault(x => x.Id == listId);

            if (shoppinglist == null)
                return new Result { Success = false, Error = "The List could not be found. Maybe it was deleted already." };
            var cont = shoppinglist.Contributors.FirstOrDefault(x => x.UserId == userId);
            if (cont == null)
                return new Result { Success = false, Error = "The user could not be found in the list" };
            if (shoppinglist.Owner.Id == userId)
                c.ShoppingLists.Remove(shoppinglist);
            else
                c.Contributors.Remove(cont);

            await FirebaseCloudMessaging.fcm.TopicMessage(shoppinglist.Id + "shoppingListTopic",
                    new { listId },
                    priority: Firebase.Priority.normal);
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

        public static async Task<Result> DeleteProduct(DBContext c, int listId, int userid, int productid)
        {
            var list = c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefault(x => x.Id == listId);
            if (list == null)
                return new Result { Success = false, Error = "list could not be found" };
            var user = list.Contributors.FirstOrDefault(x => x.UserId == userid);
            if (user == null)
                return new Result { Success = false, Error = "You are not a contributor" };
            var p = list.Products.FirstOrDefault(x => x.Id == productid);
            if (p == null)
                return new Result { Success = false, Error = "Product could not be found in the database" };
            p.Amount = 0;
            string action = "ItemDeleted";
            await FirebaseCloudMessaging.fcm.TopicMessage(list.Id + "shoppingListTopic",
                                    new { listId, p.Id, action },
                                    priority: Firebase.Priority.normal);
            await c.SaveChangesAsync();
            return new Result { Success = true };
        }

        public static async Task<Result> DeleteProducts(DBContext c, int listId, int userid, List<int> productIds)
        {
            var list = c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefault(x => x.Id == listId);
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

            string action = "Refresh";
            await FirebaseCloudMessaging.fcm.TopicMessage(list.Id + "shoppingListTopic",
                    new { listId, action },
                    priority: Firebase.Priority.normal);
            if (notFoundIds.Count == 0)
                return new Result { Success = true };
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }

        public static async Task<AddListItemResult> AddProduct(DBContext c, int listId, int userid, string name, string gtin, int amount)
        {
            var list = c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefault(x => x.Id == listId);
            if (list == null)
                return new AddListItemResult { Success = false, Error = "list could not be found" };
            var user = list.Contributors.FirstOrDefault(x => x.UserId == userid);
            if (user == null)
                return new AddListItemResult { Success = false, Error = "You are not a contributor" };
            var li = new ListItem { Gtin = gtin, Name = name, Amount = amount, ListId = listId };
            list.Products.Add(li);
            await c.SaveChangesAsync();
            //TODO Same name and same gtin

            string action = "NewItemAdded";
            await FirebaseCloudMessaging.fcm.TopicMessage(list.Id + "shoppingListTopic",
                    new { listId, li.Id, li.Name, li.Gtin, li.Amount, action },
                    priority: Firebase.Priority.normal);
            return new AddListItemResult { Success = true, Gtin = li.Gtin, Name = name, ProductId = li.Id };
        }

        //private static async Task<ShoppingList> FindListById(int id) =>
        //    await Q.From(ShoppingList.SLT).Where(ShoppingList.SLT.Id.Eq(Q.P("id", id))).FirstOrDefault<ShoppingList>(await DBConnection.OpenConnection());


        public static async Task<List<ShoppingList>> GetShoppingLists(DBContext con, int userId)
        {
            var cont = con.Contributors.Include(x => x.ShoppingList).Include(x => x.ShoppingList.Contributors).Where(x => x.User.Id == userId);
            //var list = con.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).Where(x => x.Contributors.Where(y => y.UserId == userId).ToList().Count > 1);


            var lists = new List<ShoppingList>();
            foreach (var item in cont)
            {
                var list = await LoadShoppingList(item.ShoppingList.Id, userId);
                list.Contributors = new List<Contributor>
                {
                    item
                };
                lists.Add(list);
            }

            return lists;//  con.ShoppingLists.Where(x=>x.Contributors.Contains()).ToList();

            //await con.Entry(user).Collection(m => m.ShoppingLists).LoadAsync();
            //await con.Entry(user).Collection(x => x.IsContributors).LoadAsync();
        }

        internal static async Task<ListsResult> LoadShoppingLists(DBContext con, int userId)
        {
            var lists = await GetShoppingLists(con, userId);
            var dic = lists.Select(y => new ListsResult.ListResultItem
            {
                Id = y.Id,
                Name = y.Name,
                IsAdmin = y.Contributors.FirstOrDefault().IsAdmin,
                Products = y.Products?.Select(x => new ShoppingListItemResult
                {
                    Amount = x.Amount,
                    Gtin = x?.Gtin,
                    Id = x.Id,
                    Name = x.Name
                }).ToList()
            }).ToList();

            return new ListsResult { Lists = dic };
        }
    }
}
