using NSSLServer.Database;
using NSSLServer.Models;
using System.Threading.Tasks;

namespace NSSLServer.Plugin.Shoppinglist.Sources
{
    public class LocalCacheProductSource : IProductSource
    {
        public bool Islocal { get; } = true;

        public long Total { get; set; } = 0;
        public int Priority { get; } = 5;

        public LocalCacheProductSource()
        {
        }


        //public static async void AddProduct(string name, string gtin, int quantity = 0, string unit = null)
        //{
        //    //TODO Implement saving from Outpan
        //}

        Task<IDatabaseProduct> IProductSource.FindProductByCode(string code)
        {
            return null;
        }

        public Task<Paged<IDatabaseProduct>> FindProductsByName(string name, int page = 1)
        {
            return new Task<Paged<IDatabaseProduct>>(null);
        }
    }
}
