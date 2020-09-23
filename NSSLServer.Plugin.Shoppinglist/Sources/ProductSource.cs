using Deviax.QueryBuilder;
using Deviax.QueryBuilder.Parts;

using NSSLServer.Database;
using NSSLServer.Models;
using NSSLServer.Models.DatabaseConnection;
using NSSLServer.Models.Products;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Plugin.Shoppinglist.Sources
{
    public class ProductSource : IProductSource
    {
        public bool Islocal { get; } = true;

        public async Task<IDatabaseProduct> FindProductByCode(string code)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var ret = await Q.From(GtinEntry.T)
                    .Where(x => x.Gtin.EqV(code))
                    .InnerJoin(ProductsGtins.T).On((g, pc) => g.Id.Eq(pc.GtinId))
                    .InnerJoin(Product.T).On((_, pc, p) => p.Id.Eq(pc.ProductId)).Select((g, pc, p) => new RawSql($"{p.TableAlias}.*"))
                    .OrderBy((_, __, p) => p.Fitness.Desc())
                    .FirstOrDefault<Product>(con.Connection);
                con.Connection.Close();
                return ret;
            }
        }

        public async static Task AddProduct(string name, string gtin, decimal? quantity, string unit)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            using (var tx = con.Connection.BeginTransaction())
            {
                var p = new Product { Name = name, ProviderKey = 0, Quantity = quantity, Unit = unit, };
                p.Fitness = name == name.ToUpper() ? (short)5 : (short)10;
                p.Fitness += quantity == null ? (short)0 : (short)5;
                p.Fitness += string.IsNullOrWhiteSpace(unit) ? (short)0 : (short)5;
                p.Fitness += string.IsNullOrWhiteSpace(
                    gtin) ? (short)0 : (short)10;

                await Q.InsertOne(con.Connection, tx, p);
                var g = new GtinEntry { Gtin = gtin };
                await Q.InsertOne(con.Connection, tx, g);
                var pg = new ProductsGtins { GtinId = g.Id, ProductId = p.Id };
                await Q.InsertOne(con.Connection, tx, pg);
                tx.Commit();
                con.Connection.Close();
            }
        }

        public async Task<Paged<IDatabaseProduct>> FindProductsByName(string name, int page = 1)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {

                var tsQuery = string.Join(" & ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                var q = Q.From(Product.T)
                    .Where(a => Q.ToTsVector("german", a.Name)
                    .Match(Q.ToTsQuery("german", Q.P("qry", tsQuery))));

                const int perPage = 30;

                var total = await q.Select(a => Q.Count(a.Id)).ScalarResult<long>(con.Connection);

                var items2 = q.OrderBy(a => Q.Lower(a.Name).Asc(), a => a.Fitness.Desc()).Limit(perPage, (page - 1) * perPage);
                var items = await items2.Select(new RawSql("distinct on (lower(pt.name)) *")).ToList<Product>(con.Connection);


                con.Connection.Close();
                return items.Select(x=>(IDatabaseProduct)x).ToList().Paged(total, page, perPage);

            }
        }
    }
}