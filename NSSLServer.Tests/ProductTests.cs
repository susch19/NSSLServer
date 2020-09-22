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
    public class ProductTests
    {
        [TestMethod]
        public async Task GetProductByGtin()
        {
            (var user, var listId) = await GetUserWithList();

            var product = await ProductSync.GetProduct("0000000000000");
            IsNotNull(product);
            IsTrue(product.Success);

            AreEqual(product.Gtin, "0000000000000");
            AreEqual(product.Quantity, 123);
            AreEqual(product.Unit, "T");
            AreEqual(product.Name, "!!!TESTPRODUCT!!!");
        }

        [TestMethod]
        public async Task GetProductsByName()
        {
            (var user, var listId) = await GetUserWithList();

            var productsPage1 = await ProductSync.GetProducts("!!!TESTPRODUCT!!!", 1);
            IsTrue(productsPage1.Count > 0);
            
            var productsPage2 = await ProductSync.GetProducts("!!!TESTPRODUCT!!!", 2);
            AreNotEqual(productsPage1, productsPage2);
        }

        [TestMethod]
        public async Task AddProduct()
        {
            (var user, var listId) = await GetUserWithList();

            var newProduct = await ProductSync.AddNewProduct("0000000000000", "!!!TESTPRODUCT!!!");
            IsTrue(newProduct.Success);
        }


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
