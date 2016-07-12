using Deviax.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public class BasicProduct : IDatabaseProduct
    {
        public string Name { get; set; }
        public string Gtin { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }

        public static BasicProductTable BPT = new BasicProductTable();
        public class BasicProductTable : Table<BasicProductTable>
        {
            public Field Name;
            public Field Gtin;
            public Field Quantity;
            public Field Unit;

            public BasicProductTable(string tableAlias = null) : base("public", "products", tableAlias)
            {
                Name = F("name");
                Gtin = F("gtin");
                Quantity = F("quantity");
                Unit = F("unit");
            }
        }

        public BasicProduct ConvertToProduct() => this;

        //public static implicit operator Product(BasicProduct pr)
        //{
        //    if (pr == null)
        //        return null;

        //}        
    }
}
