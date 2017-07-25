using Deviax.QueryBuilder.Parts;
using NSSLServer.Models;
using NSSLServer.Models.DatabaseConnection;
using NSSLServer.Models.Products;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deviax.QueryBuilder.Q;

namespace NSSLServer.Sources
{
    class ProductSource
    {
        public bool Islocal { get; } = true;

        public async Task<Product> FindProductByCode(string code)
         => await WithConnection(async con => await From(GtinEntry.T)
            .Where(x => x.Gtin.EqV(code))
            .InnerJoin(ProductsGtins.T).On((g, pc) => g.Id.Eq(pc.GtinId))
            .InnerJoin(Product.T).On((_, pc, p) => p.Id.Eq(pc.ProductId)).Select((g,pc,p)=>new RawSql($"{p.TableAlias}.*"))
            .OrderBy((_,__,p)=>p.Fitness.Desc())
            .FirstOrDefault<Product>(con));

        private async Task<T> WithConnection<T>(Func<DbConnection, Task<T>> f)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
                return await (f(con));
        }

        public async static Task AddProduct(string name, string gtin)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                con.Products.Add(new BasicProduct { Name = name, Gtin = gtin }); //TODO Quantity und Unit?
                await con.SaveChangesAsync();
            }
        }

        public async Task<Paged<Product>> FindProductsByName(string name, int page = 1)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
            {

                var tsQuery = string.Join(" & ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                var q = From(Product.T)
                    .Where(a => ToTsVector("german", a.Name)
                    .Match(ToTsQuery("german", P("qry", tsQuery))));

                const int perPage = 30;

                var total = await q.Select(a => Count(a.Id)).ScalarResult<long>(con);

                var items2 = q.OrderBy(a => a.Name.Asc()).Limit(perPage, (page - 1) * perPage);
                var items = await items2.ToList<Product>(con);
                return items.Paged(total, page, perPage);

            }
        }
    }
}