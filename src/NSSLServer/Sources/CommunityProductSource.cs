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
    public  static class PagedExt
    {
        public static Paged<T> Paged<T>(this List<T> l, long total, int page, int perPage) => new Paged<T>(l, total, page, perPage);
    }

    public class Paged<T>
    {
        public List<T> Items;
        public long Pages;
        public long Total;
        public int PerPage;
        public int CurrentPage;

        public Paged(List<T> items, long total, int page, int perPage)
        {
            Items = items;
            Total = total;
            CurrentPage = page;
            PerPage = perPage;

            Pages = total / perPage;
            if (total % perPage > 0)
                Pages++;
        }

        public Paged() { }
    }

    class CommunityProductSource : IProductSource
    {
       
        public bool islocal { get; } = true;

        public async Task<BasicProduct> FindProductByCode(string code)
        =>
           // Console.WriteLine(code);
            (await  Q.From(BasicProduct.BPT).Where(BasicProduct.BPT.Gtin.Eq(Q.P("ljds", code))).Limit(1).FirstOrDefault<BasicProduct>(await NsslEnvironment.OpenConnectionAsync()));

        //public async Task<List<BasicProduct>> FindProductsByName(string name)
        //    => await Q.From(BasicProduct.BPT).Where(BasicProduct.BPT.Name.Like(Q.P("ljds", "%" + name + "%"), LikeMode.IgnoreCase))
        //              .Limit(30).OrderBy(x=>x.Name.Asc()).ToList<BasicProduct>(await NsslEnvironment.OpenConnectionAsync());


        public async Task<Paged<BasicProduct>> FindProductsByName(string name, int page = 1)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
            {

                var tsQuery = string.Join(" & ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                var q = Q.From(BasicProduct.BPT)
                    .Where(a => Q.ToTsVector("German", a.Name).Match(Q.ToTsQuery("German", Q.P("qry", tsQuery))));
                    //.Where(new RawSql("to_tsvector('german', name) @@ to_tsquery('german', @qry)"))
                    //.WithExtraParameter(Q.P("qry", );
                
                const int perPage = 30;

                var total = await q.Select(a => Q.Count(a.Name)).ScalarResult<long>(con);

                var items = (await q.OrderBy(a => a.Name.Asc()).Limit(perPage, (page - 1) * perPage).ToList<BasicProduct>(con)).ToList();
                return items.Paged(total, page, perPage);

            }
        }



        public async static Task AddProduct(string name, string gtin)
        {
            using(var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                con.Products.Add(new BasicProduct { Name = name, Gtin = gtin }); //TODO Quantity und Unit?
                await con.SaveChangesAsync();
            }
        }

      
    }
}
