using Deviax.QueryBuilder;
using NSSLServer.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NSSLServer.Models.Products
{

    public class Product : IDatabaseProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal? Quantity { get; set; }
        public string ExternalId { get; set; }
        public short ProviderKey { get; set; }
        public short Fitness { get; set; }
        public virtual ICollection<GtinEntry> Gtins { get; set; }

        public static ProductsTable T = new ProductsTable("pt");
        [PrimaryKey(nameof(Id))]
        public class ProductsTable : Table<ProductsTable>
        {
            public Field Id;
            public Field Name;
            public Field Quantity;
            public Field Unit;
            public Field ProviderKey;
            public Field ExternalId;
            public Field Fitness;

            public ProductsTable(string alias = null) : base("test_schema", "products", alias)
            {
                Id = F("id");
                Name = F("name");
                Quantity = F("quantity");
                Unit = F("unit");
                ProviderKey = F("provider_key");
                ExternalId = F("external_id");
                Fitness = F("fitness");
            }
        }

    }

}
