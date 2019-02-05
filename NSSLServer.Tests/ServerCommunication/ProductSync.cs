using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static NSSL.ServerCommunication.HelperMethods;
using static Shared.ResultClasses;
using static Shared.RequestClasses;
using NSSL.Models;

namespace NSSLServer.Tests.ServerCommunication
{
    public static class ProductSync
    {
        private static string path = "products";


        public static async Task<ProductResult> GetProduct(string gtin)
        => await GetAsync<ProductResult>($"{path}/{gtin}");

        public static async Task<List<Product>> GetProducts(string name, int page)
        => await GetAsync<List<Product>>($"{path}/{name}?page={page}");

        public static async Task<Result> AddNewProduct(string gtin, string name)
        => await PostAsync<Result>($"{path}/", new AddNewProductArgs { Gtin = gtin, Name = name });
    }
}
