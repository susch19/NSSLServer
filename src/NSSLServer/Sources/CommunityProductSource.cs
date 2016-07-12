using Deviax.QueryBuilder;
using Deviax.QueryBuilder.Parts;
using NSSLServer.Models;
using NSSLServer.Models.DatabaseConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Sources
{
    class CommunityProductSource : IProductSource
    {
       
        public bool islocal { get; } = true;

        public async Task<BasicProduct> FindProductByCode(string code)
        =>
           // Console.WriteLine(code);
            (await  Q.From(BasicProduct.BPT).Where(BasicProduct.BPT.Gtin.Eq(Q.P("ljds", code))).FirstOrDefault<BasicProduct>(await DBConnection.OpenConnection()));

        public async Task<List<BasicProduct>> FindProductsByName(string name) =>
            (await Q.From(BasicProduct.BPT).Where(BasicProduct.BPT.Name.Like(Q.P("ljds","%"+name+"%"),LikeMode.IgnoreCase)).Limit(30)
            .ToList<BasicProduct>(await DBConnection.OpenConnection()))
            .Select(p => p).ToList();
            

        private void InternalAddProduct(string name, string gtin)
        {
            //table.Insert(new BasicProduct(name, gtin));
        }

        public static void AddProduct(string name, string gtin)
        {
            CommunityProductSource asd = (CommunityProductSource)ProductSourceManager.ProductSources.FirstOrDefault(x => x.GetType() == typeof(LocalOutpanProductSource));
            asd?.InternalAddProduct(name, gtin);
        }
    }
}
