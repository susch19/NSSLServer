using NSSLServer.Models;
using NSSLServer.Models.Products;
using NSSLServer.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Shared.ResultClasses;

namespace NSSLServer
{
    public static class ProductSourceManager
    {

        static ProductSource source = new ProductSource();

        public static async Task<ProductResult> FindProductByCode(string code)
        {
            if (code == null)
                return new ProductResult { Success = false, Error = "Code was not given" };

            var product = await source.FindProductByCode(code);
            if (product != null)
                return new ProductResult { Success = true, Gtin = code, Name = product.Name, Quantity = product.Quantity, Unit = product.Unit };

            return new ProductResult { Success = false, Error = "Product was not found" };
        }

        internal static async Task<List<Product>> FindProductsByName(string name, int page = 1)
        {
            if (name == null)
                return new List<Product>();
            page = page < 1 ? 1 : page;

            name = name.ToLower();
            List<Product> products = new List<Product>();

            var p = await source.FindProductsByName(name, page);
            if (p != null && p.Items?.Count > 0)
                products.AddRange(p.Items);
            return products;
        }


        internal static async Task<Result> AddNewProduct(string gtin, string name, decimal? quantity, string unit)
        {
            if (string.IsNullOrWhiteSpace(gtin) || string.IsNullOrWhiteSpace(name))
                return new Result { Success = false, Error = "The name or gtin was not properly inserted" };

            await ProductSource.AddProduct(name, gtin, quantity, unit);
            //await OutpanProductSource.AddProduct(name, gtin);
            return new Result { Success = true };
        }


    }
}
