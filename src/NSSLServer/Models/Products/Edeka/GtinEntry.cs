using Deviax.QueryBuilder;
using Deviax.QueryBuilder.Parts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NSSLServer.Models.EdekaProduct;

namespace NSSLServer.Models.Products
{
    public class EdekaGtinEntry
    {
        public string Gtin { get; set; }
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual EdekaProduct Product { get; set; }

        public static GtinsTable EGT = new GtinsTable("gt1");
        public static GtinsTable EGT2 = new GtinsTable("gt2");
        public class GtinsTable : Table<GtinsTable>
        {
            public Field Gtin;
            public Field ProductId;

            public GtinsTable(string alias = null) : base("edeka", "gtins", alias)
            {
                Gtin = F("gtin");
                ProductId = F("product_id");
            }
        }

        //public static readonly SelectQuery GtinsQuery = Q.From(EPT)
        //    .Select(EPT.Id,EPT.Name,new RawSql("array_agg(gt1.gtin)").As("gtins"))
        //    .InnerJoin(EGT).On(EPT.Id.Eq(EGT.ProductId))
        //    .InnerJoin(EGT2).On(EGT.ProductId.Eq(EGT2.ProductId))
        //    .GroupBy(EPT.Id, EPT.Name);
        public static readonly SelectQuery<GtinsTable> GtinsQuery = Q.From(EGT)
            .Select(EGT.ProductId, new RawSql("array_agg(gt1.gtin)").As("gtins"))
            .GroupBy(EGT.ProductId);



        //public static async Task<List<InternalGtinList>> GetGtinList(EdekaProduct p) =>
        //    await GtinsQuery.Where(EGT.ProductId.Eq(Q.P("asd",p.Id))).FirstOrDefault<InternalGtinList>(await DatabaseConnection.DBConnection.OpenConnection());


        public class InternalGtinList
        {
            public int ProductId { get; set; }
            public List<string> Gtins { get; set; }
        }
    }
}
