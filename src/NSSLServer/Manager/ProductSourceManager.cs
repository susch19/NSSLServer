using NSSLServer.Models;
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
                    //_productSources = typeof(IProductSource)
                    //                               .Assembly.GetTypes()
                    //                               .Where(t => t.IsSubclassOf(typeof(IProductSource)) && !t.IsAbstract)
                    //                               .Select(t => (IProductSource)Activator.CreateInstance(t)).ToList();
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

        internal static async Task<List<BasicProduct>> FindProductsByName(string name)
        {
            if (name == null)
                return new List<BasicProduct>(); 

            name = name.ToLower();
            foreach (IProductSource source in ProductSources)
            {

                var p =  await source.FindProductsByName(name);
                if (p != null && p.Count > 0)
                    return p;
            }
            return new List<BasicProduct>();
        }

        private static List<IProductSource> _productSources = new List<IProductSource>();

    }   
}
