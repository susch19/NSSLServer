using NSSLServer.Database;
using NSSLServer.Models;
using NSSLServer.Models.Products;
using NSSLServer.Plugin.Products.Core;
using NSSLServer.Plugin.Shoppinglist.Sources;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using static NSSLServer.Shared.ResultClasses;

namespace NSSLServer.Plugin.Shoppinglist.Manager
{
    public static class ProductSourceManager
    {

        //static ProductSource source = new ProductSource();

        public static async Task<ProductResult> FindProductByCode(string code)
        {
            if (code == null)
                return new ProductResult { Success = false, Error = "Code was not given" };

            IDatabaseProduct product = default;

            foreach (var source in ProductSources.Instance)
            {
                product = await source.FindProductByCode(code);
                if (product is not null)
                    break;
            }

            if (product is not null)
                return new ProductResult { Success = true, Gtin = code, Name = product.Name, Quantity = product.Quantity, Unit = product.Unit };

            return new ProductResult { Success = false, Error = "Product was not found" };
        }

        internal static async Task<List<IDatabaseProduct>> FindProductsByName(string name, int page = 1)
        {
            if (name == null)
                return new List<IDatabaseProduct>();
            page = page < 1 ? 1 : page;

            name = name.ToLower();
            List<IDatabaseProduct> products = new List<IDatabaseProduct>();

            foreach (var source in ProductSources.Instance)
            {
                var p = await source.FindProductsByName(name, page);
                if (p != null && p.Items?.Count > 0)
                    products.AddRange(p.Items);
            }

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
