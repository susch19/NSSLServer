using Deviax.QueryBuilder;
using System.Collections.Generic;

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

        public static short CalcFitness(string name, decimal? quantity, string unit, string gtin)
        {
            var fitness = name == name.ToUpper() ? (short)5 : (short)10;
            fitness += quantity == null ? (short)0 : (short)5;
            fitness += string.IsNullOrWhiteSpace(unit) ? (short)0 : (short)5;
            fitness += string.IsNullOrWhiteSpace(
                gtin) ? (short)0 : (short)10;
            return fitness;
        }

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
