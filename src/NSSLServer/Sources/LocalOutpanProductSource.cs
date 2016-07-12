using NSSLServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Sources
{
    public class LocalOutpanProductSource : IProductSource
    {
        public bool islocal { get; } = true;


        public LocalOutpanProductSource()
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

        public static void AddProduct(string name, string gtin)
        {
        }

        async Task<BasicProduct> IProductSource.FindProductByCode(string code)
        {
            return null;            
        }

        async Task<List<BasicProduct>> IProductSource.FindProductsByName(string name)
        {
            return new List<BasicProduct>();
        }
    }
}
