using NSSLServer.Models;
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
        public static List<IProductSource> ProductSources
        {
            get
            {
                if (_productSources == null || _productSources.Count == 0)
                {
                    var interfaceType = typeof(IProductSource);
                    _productSources = typeof(IProductSource).GetTypeInfo().Assembly.GetTypes()
                      .Where(x => interfaceType.IsAssignableFrom(x) && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract)
                      .Select(x => (IProductSource)Activator.CreateInstance(x)).ToList();
                }
                return _productSources;
            }
        }

        public static async Task<ProductResult> FindProductByCode(string code)
        {
            if (code == null)
                return new ProductResult {Success=false, Error = "Code was not given" };

            foreach(IProductSource source in ProductSources)
            {
                var product = await source.FindProductByCode(code);
                if (product != null)
                    return new ProductResult {Success= true, Gtin = code, Name = product.Name, Quantity = product.Quantity, Unit = product.Unit };
            }
            return new ProductResult { Success = false, Error = "Product was not found" };
        }

        internal static async Task<List<BasicProduct>> FindProductsByName(string name, int page = 1)
        {
            if (name == null)
                return new List<BasicProduct>();
            page = page < 1 ? 1 : page;

            name = name.ToLower();
            List<BasicProduct> products = new List<BasicProduct>();
            foreach (IProductSource source in ProductSources)
            {
                var p =  await source.FindProductsByName(name, page);
                if (p != null && p.Items?.Count > 0)
                    products.AddRange(p.Items);
            }
            return products;
        }

        internal static async Task<Result> AddNewProduct(string gtin, string name)
        {
            if (string.IsNullOrWhiteSpace(gtin) || string.IsNullOrWhiteSpace(name))
                return new Result {Success = false, Error = "The name or gtin was not properly inserted" };
                  
            await CommunityProductSource.AddProduct(name, gtin);
            await OutpanProductSource.AddProduct(name, gtin);
            return new Result { Success = true };
        }

        private static List<IProductSource> _productSources = new List<IProductSource>();

    }   
}
