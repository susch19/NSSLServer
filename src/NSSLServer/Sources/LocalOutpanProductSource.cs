using NSSLServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Sources
{
    public class LocalCacheProductSource : IProductSource
    {
        public bool islocal { get; } = true;

        public long Total { get; set; } = 0;

        public LocalCacheProductSource()
        {
        }

        public BasicProduct FindProductByCode(string code)
        {
            return null;
        }

        public BasicProduct[] FindProductsByName(string name)
        {
            return null;
        }

        private void InternalAddProduct(string name, string gtin)
        {
        }

        public static async void AddProduct(string name, string gtin, int quantity = 0, string unit = null)
        {
            //TODO Implement saving from Outpan
        }

        async Task<BasicProduct> IProductSource.FindProductByCode(string code)
        {
            return null;            
        }

        async Task<Paged<BasicProduct>> IProductSource.FindProductsByName(string name, int i)
        {
            return new Paged<BasicProduct>();
        }

    }
}
