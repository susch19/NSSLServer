using Deviax.QueryBuilder;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSSLServer.Models.Products
{
    public class ProductsGtins
    {
        public int GtinId { get; set; }
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual List<Product> Products { get; set; }
        [ForeignKey(nameof(GtinId))]
        public virtual List<GtinEntry> Gtins { get; set; }


        public static ProductsGtinsTable T = new ProductsGtinsTable("gpt");
        [PrimaryKey(nameof(GtinId), nameof(ProductId))]
        public class ProductsGtinsTable : Table<ProductsGtinsTable>
        {
            public Field GtinId;
            public Field ProductId;

            public ProductsGtinsTable(string alias = null) : base("test_schema", "products_gtins", alias)
            {
                GtinId = F("gtin_id");
                ProductId = F("product_id");
            }
        }

        //public static readonly SelectQuery GtinsQuery = Q.From(EPT)
        //    .Select(EPT.Id,EPT.Name,new RawSql("array_agg(gt1.gtin)").As("gtins"))
        //    .InnerJoin(EGT).On(EPT.Id.Eq(EGT.ProductId))
        //    .InnerJoin(EGT2).On(EGT.ProductId.Eq(EGT2.ProductId))
        //    .GroupBy(EPT.Id, EPT.Name);
        //public static readonly SelectQuery<GtinsTable> GtinsQuery = Q.From(EGT)
        //    .Select(EGT.ProductId, new RawSql("array_agg(gt1.gtin)").As("gtins"))
        //    .GroupBy(EGT.ProductId);

        public override string ToString()
        {
            return $"{GtinId}|{ProductId}";
        }

        //public static async Task<List<InternalGtinList>> GetGtinList(EdekaProduct p) =>
        //    await GtinsQuery.Where(EGT.ProductId.Eq(Q.P("asd",p.Id))).FirstOrDefault<InternalGtinList>(await DatabaseConnection.DBConnection.OpenConnection());
       
    }
}
