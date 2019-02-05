using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSSL.Models;
using NSSLServer.Tests.ServerCommunication;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace NSSLServer.Tests
{
    [TestClass]
    public class ShoppingListTests
    {
        [TestMethod]
        public async Task GetList()
        {
            (var user, var listId) = await GetUserWithList();

            var list = await ShoppingListSync.GetList(listId);

            IsNotNull(list);
            AreEqual(list.Id, listId);
            IsNotNull(list.Name);
            IsNotNull(list.Products);
        }

        [TestMethod]
        public async Task GetLists()
        {
            (var user, var listId) = await GetUserWithList();

            var lists = await ShoppingListSync.GetLists();

            IsNotNull(lists);
        }

        [TestMethod]
        public async Task ChangeRights()
        {
            (var user, var listId) = await GetUserWithList();

            var contributors = await ShoppingListSync.GetContributors(listId);
            var contributor = contributors.Contributors.FirstOrDefault(c => c.UserId != user.Id);
            IsNotNull(contributor, "No other contributors on this list");

            var res = await ShoppingListSync.ChangeRights(listId, contributor.UserId);

            IsTrue(res.Success, res.Error);
        }

        [TestMethod]
        public async Task DeleteContributor()
        {
            (var user, var listId) = await GetUserWithList();

            var contributors = await ShoppingListSync.GetContributors(listId);
            var contributor = contributors.Contributors.FirstOrDefault(c => c.UserId != user.Id);
            IsNotNull(contributor, "No other contributors on this list");

            var res = await ShoppingListSync.DeleteContributor(listId, contributor.UserId);

            IsTrue(res.Success, res.Error);
        }

        [TestMethod]
        public async Task AddContributor()
        {
            (var user, var listId) = await GetUserWithList();

            var contributors = await ShoppingListSync.AddContributor(listId, "test-user_03.02.2018_13-11");
            IsNotNull(contributors);
            IsTrue(contributors.Success, contributors.Error);
        }

        [TestMethod]
        public async Task GetContributors()
        {
            (var user, var listId) = await GetUserWithList();

            var contributors = await ShoppingListSync.GetContributors(listId);
            IsNotNull(contributors);
            IsTrue(contributors.Success);
            IsTrue(contributors.Contributors.Count > 0);
        }

        [TestMethod]
        public async Task ChangeProduct()
        {
            (var user, var listId) = await GetUserWithList();


            var list = await ShoppingListSync.GetList(listId);
            IsNotNull(list.Products);
            IsTrue(list.Products.Count > 0);

            var changeProduct = list.Products.FirstOrDefault();
            var newName = "TEST PRODUCT - " + DateTime.Now.ToString();
            var newProduct = await ShoppingListSync.ChangeProduct(listId, changeProduct.ID, 1, newName);

            IsTrue(newProduct.Success);
            AreEqual(changeProduct.Amount + 1, newProduct.Amount);
            AreEqual(changeProduct.ID, newProduct.Id);
            AreNotEqual(changeProduct.Name, newProduct.Name);
            AreEqual(newProduct.Name, newName);
        }

        [TestMethod]
        public async Task RenameList()
        {
            (var user, var listId) = await GetUserWithList();

            var newName = "renamed testliste";

            var renameResult = await ShoppingListSync.RenameList(listId, newName);
            IsTrue(renameResult.Success);
            AreEqual(renameResult.Name, newName, "New Name was set incorrectly");
            AreEqual(renameResult.ListId, listId, "List Ids don't match");
        }

        [TestMethod]
        public async Task DeleteList()
        {

            (var user, var listId) = await GetUserWithList();


            var deleteResult =  await ShoppingListSync.DeleteList(listId);
            IsTrue(deleteResult.Success, "List couldn't be deleted");
        }

        [TestMethod]
        public async Task AddList()
        {
            var user = User.GetUser(out var success);
            IsTrue(success);

            var list = ShoppingListSync.AddList("testliste").Result;
            IsNotNull(list);
            IsNotNull(list.Id);
            IsNotNull(list.Name);
        }

        [TestMethod]
        public async Task DeleteProduct()
        {
            (var user, var listId) = await GetUserWithList();

            var list = await ShoppingListSync.GetList(listId);
            IsNotNull(list.Products);
            IsTrue(list.Products.Count > 0, "No Products to delete are on the list");

            var id = list.Products.FirstOrDefault().ID;
            var delete = await ShoppingListSync.DeleteProduct(listId, id);

            IsTrue(delete.Success);

            list = await ShoppingListSync.GetList(listId);
            IsNotNull(list.Products);
            IsNull(list.Products.FirstOrDefault(x=>x.ID == id));
        }

        [TestMethod]
        public async Task BatchActionDelete()
        {
            (var user, var listId) = await GetUserWithList();
            
            var list = await ShoppingListSync.GetList(listId);
            IsNotNull(list.Products);
            IsTrue(list.Products.Count > 0, "No Products to delete are on the list");

            var delete = await ShoppingListSync.DeleteProducts(listId, list.Products.Select(x => x.ID).ToList());

            IsTrue(delete.Success);
            IsNull(delete.productIds);

            list = await ShoppingListSync.GetList(listId);
            IsNotNull(list.Products);
            IsTrue(list.Products.Count == 0);
        }

        [TestMethod]
        public async Task BatchActionChange()
        {
            (var user, var listId) = await GetUserWithList();

            var list = await ShoppingListSync.GetList(listId);
            IsNotNull(list.Products);
            IsTrue(list.Products.Count > 0);

            var change = await ShoppingListSync.ChangeProducts(listId, list.Products.Select(x=>x.ID).ToList(), list.Products.Select(x => 5).ToList());
            int hash = 0;
            list.Products.ForEach(x => hash += x.ID + x.Amount + 5);
            AreEqual(change.Hash, hash);
        }

        [TestMethod]
        public async Task AddProduct()
        {
            (var user, var listId) = await GetUserWithList();

            var product = await ShoppingListSync.AddProduct(listId, "TestProduct", "0000000000000", 1);
            IsNotNull(product);
            AreEqual(product.Gtin, "0000000000000");
            AreEqual(product.Name, "TestProduct");
        }

        //[TestMethod]
        //public async Task OurList()
        //{
        //    NSSL.ServerCommunication.HelperMethods.Token = "ewogICJ0eXAiOiAiSldUIiwKICAiYWxnIjogIkhTMjU2Igp9.ewogICJleHBpcmVzIjogIjIwMTgtMDQtMDVUMTg6MjM6NTEuODY3NjY1WiIsCiAgImlkIjogNDUsCiAgImNyZWF0ZWQiOiAiMjAxOC0wMy0wNVQxODoyMzo1MS44Njc3MTI5WiIKfQ.MMNziqD-vW0oa69RyTY-tw4sJYOM-FCiCpM2IAQdLRw";

        //    var info = await UserSync.Info();

        //    var list = info.ListIds.FirstOrDefault();

        //    await ShoppingListSync.AddProduct(list, "Hackfleisch Gemischt 1kg (Theke => Angebot)", null, 1);
        //    await ShoppingListSync.AddProduct(list, "Kasseler Schweinelachs 300g abgepackt", null, 2);
        //    await ShoppingListSync.AddProduct(list, "Stangenporree", null, 5);
        //    await ShoppingListSync.AddProduct(list, "Milch", null, 5);
        //    await ShoppingListSync.AddProduct(list, "Sonnen Blutorangen 1,99€", null, 1);
        //    await ShoppingListSync.AddProduct(list, "Asia Nudeln", null, 1);
        //    await ShoppingListSync.AddProduct(list, "Chips im Angebot", null, 1);
        //    await ShoppingListSync.AddProduct(list, "Eistee Patrick", null, 1);
        //    await ShoppingListSync.AddProduct(list, "Salmiakpastillen", null, 1);
        //}

        public async Task<(User user, int listId)> GetUserWithList(bool withToken = true)
        {
            User user;
            if (withToken)
            {
                user = User.GetUser(out var success);
                IsTrue(success, "Token is missing");
            }
            else
            {
                user = User.GetUser();
            }

            var info = await UserSync.Info();
            IsNotNull(info.ListIds, "The User has no lists");

            var list = info.ListIds.FirstOrDefault();
            IsNotNull(list, "The User has no lists");

            return (user, list);
        }

    }
}
