using Deviax.QueryBuilder.Parts;
using NSSLServer.Models;
using NSSLServer.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deviax.QueryBuilder.Q;
using static NSSLServer.Models.EdekaProduct;
using static NSSLServer.Models.DatabaseConnection.DBConnection;
using Deviax.QueryBuilder;

namespace NSSLServer.Sources
{
    class EdekaProductSource : IProductSource
    {
        public bool islocal { get; } = true;
        
        private static async Task<EdekaGtinEntry> GetId(string code)
         => await From(EdekaGtinEntry.EGT).Where(EdekaGtinEntry.EGT.Gtin.Eq(P("q", code))).FirstOrDefault<EdekaGtinEntry>(await OpenConnection());

        public async Task<BasicProduct> FindProductByCode(string code)
        => (await From(EdekaGtinEntry.EGT).Where((x)=>x.Gtin.Eq(code)).InnerJoin(EPT).On((x,y)=> x.ProductId.Eq(y.Id)).Select(new RawSql("ept.*")).Limit(1).FirstOrDefault<EdekaProduct>(await OpenConnection()))?.ConvertToProduct();

        //public async Task<List<BasicProduct>> FindProductsByName(string name)
        // =>( await From(EPT).Where(EPT.Name.Like(P("gtin", "%" + name +"%"), LikeMode.IgnoreCase)).Limit(30).ToList<EdekaProduct>(await OpenConnection())).Select(p => p.ConvertToProduct()).ToList();

        public async Task<Paged<BasicProduct>> FindProductsByName(string name, int page = 1)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
            {

                var tsQuery = string.Join(" & ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                var q = Q.From(EPT)
                    .Where(a => Q.ToTsVector(Regconfig.German, Q.Concat(a.Name, a.LongDescription, a.ShortDescription))
                    .Match(Q.ToTsQuery(Regconfig.German, Q.P("qry", tsQuery))));
                    
                    //.Where(EPT.Name.Like(P("gtin", "%" + name + "%"), LikeMode.IgnoreCase));
                    
                const int perPage = 30;
              
                var total = await q.Select(a => Q.Count(a.Id)).ScalarResult<long>(con);

                var items = (await q.OrderBy(a => a.Name.Asc()).Limit(perPage, (page - 1) * perPage).ToList<EdekaProduct>(con)).Select(p => p.ConvertToProduct()).ToList();
                return items.Paged(total, page, perPage);

            }
        }
    }
}