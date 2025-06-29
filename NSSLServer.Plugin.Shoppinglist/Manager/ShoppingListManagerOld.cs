using Deviax.QueryBuilder;
using Deviax.QueryBuilder.ChangeTracking;
using Deviax.QueryBuilder.Parts;
using NSSLServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NSSLServer.Shared.ResultClasses;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
namespace NSSLServer.Plugin.Shoppinglist.Manager
{
    public static class ShoppingListManagerOld
    {
        public static async Task<ShoppingList> LoadShoppingList(DBContext c, int listId, bool alreadyBought, int userId)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var list = await Q.From(Contributor.T)
                    .InnerJoin(ShoppingList.T).On((c, sl) => c.ListId.Eq(sl.Id))
                    .Where(
                        (c, sl) => c.UserId.EqV(userId),
                        (c, sl) => sl.Id.EqV(listId)
                    )
                    .Select(new RawSql(ShoppingList.T.TableAlias + ".*")).Limit(1)
                    .FirstOrDefault<ShoppingList>(con.Connection);

                if (list == null)
                    return new ShoppingList { }; //TODO Nicht leere Liste zurückgeben

                var tempQuery = Q.From(ListItem.T).Where(l => l.ListId.EqV(list.Id)).OrderBy(t => t.Id.Asc());

                if (!alreadyBought)
                    list.Products = await tempQuery.Where(l => l.Amount.Neq(Q.P("a", 0))).ToList<ListItem>(con.Connection);
                else
                {
                    list.Products = await tempQuery.Where(l => l.Amount.Eq(Q.P("a", 0))).ToList<ListItem>(con.Connection);
                    foreach (var item in list.Products)
                    {
                        item.Amount = item.BoughtAmount;
                    }
                }

                //list.Products = await Q.From(ListItem.T).Where(l => l.ListId.EqV(list.Id))
                //        .OrderBy(t => t.Id.Asc())
                //        .Where(l => l.Amount.Neq(Q.P("a", 0)))
                //        .ToList<ListItem>(con.Connection);
                con.Connection.Close();
                return list;
            }
        }

        public static async Task<Result> ChangeRights(DBContext c, int listId, int requesterId, int changeUserId)
        {
            //var list = await Q.From(ShoppingList.T).Where(x => x.UserId.EqV(requesterId), x => x.Id.EqV(listId)).FirstOrDefault<ShoppingList>(c.Connection);

            var contributors = await Q.From(Contributor.T).Where(x => x.ListId.EqV(listId), x => x.UserId.InV(new[] { requesterId, changeUserId })).ToList<Contributor>(c.Connection);// list.Contributors.FirstOrDefault(x => x.UserId == requesterId);

            var requester = contributors.FirstOrDefault(x => x.UserId == requesterId);
            var changeUser = contributors.FirstOrDefault(x => x.UserId == changeUserId);

            if (requester == null || changeUser == null)
                return new Result { Error = "Either you or the other user was not part of the list" };

            if (!Contributor.Permissions.CanChange(requester.Permission, changeUser.Permission))
                return new Result { Error = "You are not an admin or you wanted to demote the owner" };

            ChangeTrackingContext ctc = new ChangeTrackingContext();
            ctc.Track(changeUser);
            changeUser.Permission = Contributor.Permissions.IsAdmin(changeUser.Permission) ? Contributor.Permissions.User : Contributor.Permissions.Admin;
            await ctc.Commit(c.Connection);
            return new Result { Success = true };
        }

        public static async Task<Result> DeleteContributor(DBContext c, int listId, int adminId, int contributorId)
        {
            var list = await Q.From(ShoppingList.T)
                .Where(x => x.Id.EqV(listId))
                .Where(x => Q.Exists(Q.From(Contributor.T)
                    .Where(a => a.ListId.Eq(x.Id),
                        a => a.UserId.EqV(adminId),
                        a => a.Permission.Gte(Q.From(new Contributor.ContributorTable("c2"))
                            .Where(d => d.UserId.EqV(contributorId, "contributorId"),
                                d => d.ListId.Eq(x.Id))
                            .Select(d => d.Permission)))
                .Select(new RawSql("null")))).FirstOrDefault<ShoppingList>(c.Connection);

            if (list == null)
                return new Result { Success = false, Error = "insufficient rights" };
            await Q.DeleteFrom(Contributor.T).Where(x => x.UserId.EqV(contributorId), x => x.ListId.EqV(listId)).Execute(c.Connection); //TODO WTF?!?!?!
            return new Result { Success = true };
        }

        public static async Task<AddContributorResult> AddContributor(DBContext c, int listId, int userId, User contributor, string deviceToken)
        {
            var list = await Q.From(ShoppingList.T).Where(x => x.Id.EqV(listId)).Where(x => Q.Exists(Q.From(Contributor.T).Where(a => a.ListId.Eq(x.Id), a => a.UserId.EqV(userId), a => a.Permission.GteV(Contributor.Permissions.Admin)).Select(new RawSql("null")))).FirstOrDefault<ShoppingList>(c.Connection);

            if (list == null)
                return new AddContributorResult { Success = false, Error = "insufficient rights" };

            var existingContributor = await Q.From(Contributor.T).Where(x => x.UserId.EqV(contributor.Id), x => x.ListId.EqV(listId)).FirstOrDefault<Contributor>(c.Connection);
            if (existingContributor != null)
                return new AddContributorResult { Success = false, Error = "user is already contributor" };
            var con = new Contributor
            {
                UserId = contributor.Id,
                Permission = Contributor.Permissions.User,
                ListId = listId
            };
            await Q.InsertOne(c.Connection, con);

            List<ListItem> items;
            items = await Q.From(ListItem.T).Where(li => li.ListId.EqV(listId), li => li.Amount.NeqV(0)).ToList<ListItem>(c.Connection);


            FirebaseCloudMessaging.TopicMessage($"{contributor.Username}userTopic", "You were added to the list " + list.Name, null);
            FirebaseCloudMessaging.TopicMessage($"{contributor.Username}userTopic", new { userId, listId, list.Name, items, deviceToken });

            return new AddContributorResult { Success = true, Id = contributor.Id, Name = contributor.Username };
        }

        public static async Task<GetContributorsResult> GetContributors(DBContext c, int listId, int userId)
        {
            var cont = await Q.From(Contributor.T)
                .Where(a => a.ListId.EqV(listId))
                .Where(a => Q.Exists(
                    Q.From(Contributor.T)
                    .InnerJoin(ShoppingList.T)
                    .On((d, s) => s.Id.Eq(d.ListId))
                    .Where((d, s) => d.UserId.EqV(userId))
                    .Select(new RawSql("null")))
                ).InnerJoin(User.T).On((co, u) => co.UserId.Eq(u.Id)).Select(
                    (x, y) => x.UserId,
                    (x, y) => y.Username.As("name"),
                    (x, y) => x.Permission.GteV(Contributor.Permissions.Admin).As("is_admin"))
                .ToList<GetContributorsResult.ContributorResult>(c.Connection);

            if (cont == null)
                return new GetContributorsResult { Success = false, Error = "User is not part of the list" };
            return new GetContributorsResult
            {
                Success = true,
                Contributors = cont
            };
        }

        public static async Task<ChangeListItemResult> ChangeProduct(DBContext c, int listId, int userId, int productId, int change, int? order, string newName, string deviceToken)
        {
            //var cont = await Q.From(Contributor.T).Where(x => x.ListId.EqV(listId), x => x.UserId.EqV(userId)).FirstOrDefault<Contributor>(c.Connection);
            //if (cont == null)
            //    return new ChangeListItemResult { Success = false, Error = "User is not allowed to access this list" };
            var query = Q.From(ListItem.T)
                .InnerJoin(Contributor.T)
                .On((l, c) => l.ListId.Eq(c.ListId))
                .Where((l, c) => l.Id.EqV(productId), (l, c) => l.ListId.EqV(listId), (l, c) => c.UserId.EqV(userId))
                .Select((l, c) => new RawSql(l.TableAlias + ".*"));
            var product = await query
                .FirstOrDefault<ListItem>(c.Connection);
            if (product == null)
                return new ChangeListItemResult { Success = false, Error = "Product not found" };

            string action;
            var ctc = new ChangeTrackingContext();
            ctc.Track(product);
            if (change != 0)
            {
                if (product.Amount + change <= 0 || change == 0)
                {
                    product.BoughtAmount = product.Amount;
                    product.Amount = 0;
                    action = "ItemDeleted";
                    FirebaseCloudMessaging.TopicMessage($"{product.ListId}shoppingListTopic", new { userId, product.ListId, product.Id, action, deviceToken });
                }
                else
                {
                    product.BoughtAmount = 0;
                    product.Amount += change;
                    if (product.Amount - change == 0)
                    {
                        action = "NewItemAdded";
                        FirebaseCloudMessaging.TopicMessage($"{product.ListId}shoppingListTopic", new { userId, product.ListId, product.Id, product.Amount, product.Name, product.SortOrder, action, deviceToken });
                    }
                    else
                    {
                        action = "ItemChanged";
                        FirebaseCloudMessaging.TopicMessage($"{product.ListId}shoppingListTopic", new { userId, product.ListId, product.Id, product.Amount, product.SortOrder, action, deviceToken });
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(newName))
            {
                product.Name = newName;
                action = "ItemRenamed";
                FirebaseCloudMessaging.TopicMessage($"{product.ListId}shoppingListTopic", new { userId, product.ListId, product.Id, product.Name, action, deviceToken });
            }
            if (order.HasValue)
            {
                product.SortOrder = order.Value;
                action = "OrderChanged";
                FirebaseCloudMessaging.TopicMessage($"{product.ListId}shoppingListTopic", new { userId, product.ListId, product.Id, product.SortOrder, action, deviceToken });
            }
            await ctc.Commit(c.Connection);
            return new ChangeListItemResult
            {
                Success = true,
                Id = product.Id,
                Name = product.Name,
                Amount = product.Amount??0,
                Order = product.SortOrder,
                Changed = product.Changed ?? DateTime.MinValue,
                ListId = product.ListId
            };
        }

        public static async Task<Result> ChangeProducts(DBContext c, int listId, int userId, List<int> productIds, List<int> changes, string deviceToken)
        {
            if (productIds.Count != changes.Count)
                return new Result { Success = false, Error = "Length of product ids doesn't match with length of change list" };

            var products = await Q.From(ListItem.T).InnerJoin(Contributor.T).On((x, y) => x.ListId.Eq(y.ListId))
                .Where((a, s) => a.ListId.EqV(listId), (a, s) => s.UserId.EqV(userId)).ToList<ListItem>(c.Connection);
            if (products == null)
                return new Result { Success = false, Error = "User is not allowed to access this list" };

            //var shoppinglist = await c.ShoppingLists.Include(x => x.Contributors).Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == listId);

            var notFoundIds = new List<int>();
            int hash = 0;
            var ctc = new ChangeTrackingContext();
            for (int i = 0; i < productIds.Count; i++)
            {
                var id = productIds[i];
                var change = changes[i];
                var product = products.FirstOrDefault(x => x.Id == id);
                if (product == null)
                    notFoundIds.Add(id);
                else
                {
                    ctc.Track(product);
                    if (product.Amount + change <= 0 || change == 0)
                    {
                        product.BoughtAmount = product.Amount;
                        product.Amount = 0;
                    }
                    else
                        product.Amount += change;
                    hash += product.Amount ?? 0 + product.Id;
                }
            }
            await ctc.Commit(c.Connection);
            string action = "Refresh";
            FirebaseCloudMessaging.TopicMessage($"{listId}shoppingListTopic", new { userId, listId, action, deviceToken });

            if (notFoundIds.Count == 0)
                return new HashResult { Success = true, Hash = hash };
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }

        public static async Task<Result> ReorderProducts(DBContext c, int listId, int userId, List<int> productIds, string deviceToken)
        {
            var products = await Q.From(ListItem.T).InnerJoin(Contributor.T).On((x, y) => x.ListId.Eq(y.ListId))
               .Where((a, s) => a.ListId.EqV(listId), (a, s) => s.UserId.EqV(userId)).Select((x,y)=>new RawSql(x.TableAlias + ".*")).ToList<ListItem>(c.Connection);
            if (products == null)
                return new Result { Success = false, Error = "User is not allowed to access this list" };

            var notFoundIds = new List<int>();
            int hash = 0;
            var ctc = new ChangeTrackingContext();
            for (int i = 1; i <= productIds.Count; i++)
            {
                var id = productIds[i-1];
                var product = products.FirstOrDefault(x => x.Id == id);
                if (product == null)
                    notFoundIds.Add(id);
                else
                {
                    ctc.Track(product);
                    product.SortOrder = i;
                    hash += product.Id * i;
                }
            }
            await ctc.Commit(c.Connection);
            string action = "Refresh";
            FirebaseCloudMessaging.TopicMessage($"{listId}shoppingListTopic", new { userId, listId, action, deviceToken });

            if (notFoundIds.Count == 0)
                return new HashResult { Success = true, Hash = hash };
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }

        public static async Task<ChangeListNameResult> ChangeListname(DBContext c, int id, int userId, string newName, string deviceToken)
        {
            var list = await Q.From(ShoppingList.T).Where(x => x.Id.EqV(id)).Where(x => Q.Exists(Q.From(Contributor.T).Where(a => a.ListId.Eq(x.Id), a => a.UserId.EqV(userId), a => a.Permission.GteV(Contributor.Permissions.Admin)).Select(new RawSql("null")))).FirstOrDefault<ShoppingList>(c.Connection);
            if (list == null)
                return new ChangeListNameResult { Success = false, Error = "Insufficient rights" };
            var ctc = ChangeTrackingContext.StartWith(list);

            list.Name = newName;
            var listId = list.Id;

            string action = "ListRename";
            FirebaseCloudMessaging.TopicMessage($"{list.Id}shoppingListTopic", new { userId, listId, list.Name, action, deviceToken });

            await ctc.Commit(c.Connection);
            return new ChangeListNameResult { Success = true, ListId = list.Id, Name = list.Name };
        }

        public static async Task<Result> DeleteList(DBContext c, int listId, int userId, string deviceToken)
        {
            var cont = await Q.From(Contributor.T).Where(a => a.ListId.EqV(listId), a => a.UserId.EqV(userId)).FirstOrDefault<Contributor>(c.Connection);

            if (cont == null)
                return new Result { Success = false, Error = "The List could not be found or you are not a contributor." };

            if (cont.Permission == Contributor.Permissions.Owner)
            {
                using (var tx = c.Connection.BeginTransaction())
                {
                    await Q.DeleteFrom(ListItem.T).Where(x => x.ListId.EqV(listId)).Execute(c.Connection, tx);
                    await Q.DeleteFrom(Contributor.T).Where(x => x.ListId.EqV(listId)).Execute(c.Connection, tx);
                    await Q.DeleteFrom(ShoppingList.T).Where(x => x.Id.EqV(listId)).Execute(c.Connection, tx);
                    tx.Commit();
                }
            }
            else
            {
                //Q.Create(Contributor.T).Column(x => x.Permission).Type("asd");
                //await Q.DeleteFrom(Contributor.T).Where(x => x.ListId.EqV(listId), x => x.UserId.EqV(userId)).Execute(c.Connection);
            }

            FirebaseCloudMessaging.TopicMessage($"{listId}shoppingListTopic", new { userId, listId });
            return new Result { Success = true };
        }

        public static async Task<AddListResult> AddList(DBContext c, string listName, int userId)
        {
            var user = await Q.From(User.T).Where(x => x.Id.EqV(userId)).FirstOrDefault<User>(c.Connection);// c.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new AddListResult { Success = false, Error = "User not found" };

            var shoppingList = new ShoppingList { Name = listName };
            await Q.InsertOne(c.Connection, shoppingList);
            var cont = new Contributor { ListId = shoppingList.Id, Permission = Contributor.Permissions.Owner, UserId = userId };
            await Q.InsertOne(c.Connection, cont);

            return new AddListResult { Success = true, Id = shoppingList.Id, Name = shoppingList.Name };
        }

        public static async Task<Result> DeleteProduct(DBContext c, int listId, int userId, int productId, string deviceToken)
        {
            var cont = await Q.From(Contributor.T).Where(a => a.ListId.EqV(listId), a => a.UserId.EqV(userId)).FirstOrDefault<Contributor>(c.Connection);

            if (cont == null)
                return new Result { Success = false, Error = "You are not a contributor" };
            var p = await Q.From(ListItem.T).Where(x => x.ListId.EqV(listId), x => x.Id.EqV(productId)).FirstOrDefault<ListItem>(c.Connection);// list.Products.FirstOrDefault(x => x.Id == productId);
            if (p == null)
                return new Result { Success = false, Error = "Product was not found" };
            var ctc = ChangeTrackingContext.StartWith(p);
            p.BoughtAmount = p.Amount;
            p.Amount = 0;
            string action = "ItemDeleted";
            FirebaseCloudMessaging.TopicMessage($"{listId}shoppingListTopic", new { userId, listId, p.Id, action, deviceToken });
            await ctc.Commit(c.Connection);
            return new Result { Success = true };
        }

        public static async Task<Result> DeleteProducts(DBContext c, int listId, int userId, List<int> productIds, string deviceToken)
        {
            var cont = await Q.From(Contributor.T).Where(a => a.ListId.EqV(listId), a => a.UserId.EqV(userId)).FirstOrDefault<Contributor>(c.Connection);

            if (cont == null)
                return new Result { Success = false, Error = "You are not a contributor " };
            var notFoundIds = new List<int>();
            var p = await Q.From(ListItem.T).Where(x => x.ListId.EqV(listId), x => x.Id.InV(productIds)).ToList<ListItem>(c.Connection);
            var ctc = new ChangeTrackingContext();

            foreach (var product in p)
            {
                ctc.Track(product);
                if (product == null)
                    notFoundIds.Add(product.Id);
                else
                {
                    product.BoughtAmount = product.Amount;
                    product.Amount = 0;
                }
            }
            await ctc.Commit(c.Connection);

            string action = "Refresh";
            FirebaseCloudMessaging.TopicMessage($"{listId}shoppingListTopic", new { userId, listId, action, deviceToken });
            if (notFoundIds.Count == 0)
                return new Result { Success = true };
            else
                return new DeleteProductsResult { Success = true, Error = "Some Products could not be found in the Database", productIds = notFoundIds };
        }


        public static async Task<AddListItemResult> AddProduct(DBContext c, int listId, int userId, string name, string gtin, int amount, int? order, string deviceToken)
        {
            var cont = await Q.From(Contributor.T).Where(a => a.ListId.EqV(listId), a => a.UserId.EqV(userId)).FirstOrDefault<Contributor>(c.Connection);
            if (cont == null)
                return new AddListItemResult { Success = false, Error = "You are not a contributor" };

            if (!order.HasValue)
                order = (int)(await Q.From(ListItem.T).Where(x => x.ListId.EqV(listId)).Select(x => Q.Count(x.Id)).FirstOrDefault<DbFunctionModel>(c.Connection)).Count +1;

            var li = new ListItem { Gtin = gtin, Name = name, Amount = amount, ListId = listId, SortOrder = order.Value, Created = DateTime.UtcNow };
            await Q.InsertOne(c.Connection, li);
            //TODO Same name and same gtin <-- WTF?

            string action = "NewItemAdded";
            FirebaseCloudMessaging.TopicMessage($"{listId}shoppingListTopic", new { userId, listId, li.Id, li.Name, li.Gtin, li.Amount, li.SortOrder, action, deviceToken });
            return new AddListItemResult { Success = true, Gtin = li.Gtin, Name = name, ProductId = li.Id };
        }

        //private static async Task<ShoppingList> FindListById(int id) =>
        //    await Q.From(ShoppingList.SLT).Where(ShoppingList.SLT.Id.Eq(Q.P("id", id))).FirstOrDefault<ShoppingList>(await DBConnection.OpenConnection());



        internal static async Task<ListsResult> LoadShoppingLists(DBContext con, int userId)
        {

            var lists = await Q.From(Contributor.T)
                .Where(x => x.UserId.EqV(userId))
                .InnerJoin(ShoppingList.T).On((x, y) => x.ListId.Eq(y.Id))
                .Select(
                    (x, y) => y.Id,
                    (x, y) => y.Name,
                    (x, y) => x.Permission.GteV(Contributor.Permissions.Admin).As(N.Db(nameof(ListsResult.ListResultItem.IsAdmin)))
                )
                .ToList<ListsResult.ListResultItem>(con.Connection);
            foreach (var g in (await Q.From(ListItem.T).Where(x => x.ListId.InV(lists.Select(y => y.Id)), x => x.Amount.GtV(0)).ToList<ListItem>(con.Connection)).GroupBy(a => a.ListId))
                lists.First(x => x.Id == g.Key).Products = g.OrderBy(x=>x.SortOrder).ThenBy(x=>x.Id).Select(x => new ShoppingListItemResult
                {
                    Amount = x.Amount ?? 0,
                    Gtin = x?.Gtin,
                    Id = x.Id,
                    Name = x.Name,
                    Order = x.SortOrder,
                    Changed = x.Changed ?? DateTime.MinValue,
                    Created = x.Created ?? DateTime.MinValue
                }).ToList();
            foreach (var item in lists)
                if (item.Products == null)
                    item.Products = new List<ShoppingListItemResult>();

            return new ListsResult { Lists = lists };
        }
    }
}

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed