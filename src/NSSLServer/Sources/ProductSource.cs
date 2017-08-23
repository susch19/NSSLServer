using Deviax.QueryBuilder;
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
            .InnerJoin(Product.T).On((_, pc, p) => p.Id.Eq(pc.ProductId)).Select((g, pc, p) => new RawSql($"{p.TableAlias}.*"))
            .OrderBy((_, __, p) => p.Fitness.Desc())
            .FirstOrDefault<Product>(con));

        private async Task<T> WithConnection<T>(Func<DbConnection, Task<T>> f)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
                return await (f(con));
        }

        public async static Task AddProduct(string name, string gtin, decimal? quantity, string unit)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
            using (var tx = con.BeginTransaction())
            {
                var p = new Product { Name = name, ProviderKey = 0, Quantity = quantity, Unit = unit, };
                p.Fitness = name == name.ToUpper() ? (short)5 : (short)10;
                p.Fitness += quantity == null ? (short)0 : (short)5;
                p.Fitness += string.IsNullOrWhiteSpace(unit) ? (short)0 : (short)5;
                p.Fitness += string.IsNullOrWhiteSpace(
                    gtin) ? (short)0 : (short)10;

                await Q.InsertOne(con, tx, p);
                var g = new GtinEntry { Gtin = gtin };
                await Q.InsertOne(con, tx, g);
                var pg = new ProductsGtins { GtinId = g.Id, ProductId = p.Id };
                await Q.InsertOne(con, tx, pg);
                tx.Commit();
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

                var items2 = q.OrderBy(a => Lower(a.Name).Asc(), a => a.Fitness.Desc()).Limit(perPage, (page - 1) * perPage);
                var items = await items2.Select(new RawSql("distinct on (lower(pt.name)) *")).ToList<Product>(con);
                return items.Paged(total, page, perPage);

            }
        }
    }
}