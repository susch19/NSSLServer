using Deviax.QueryBuilder;
using Deviax.QueryBuilder.Parts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models.Products
{
    public class GtinEntry
    {
        public string Gtin { get; set; }
        public int Id { get; set; }
        
        public static GtinsTable T = new GtinsTable("gt");
        [PrimaryKey(nameof(Id))]
        public class GtinsTable : Table<GtinsTable>
        {
            public Field Gtin;
            public Field Id;

            public GtinsTable(string alias = null) : base("test_schema", "gtins", alias)
            {
                Gtin = F("gtin");
                Id = F("id");
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
            return $"{Gtin}|{Id}";
        }

        //public static async Task<List<InternalGtinList>> GetGtinList(EdekaProduct p) =>
        //    await GtinsQuery.Where(EGT.ProductId.Eq(Q.P("asd",p.Id))).FirstOrDefault<InternalGtinList>(await DatabaseConnection.DBConnection.OpenConnection());
       
    }
}
