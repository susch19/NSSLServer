using Deviax.QueryBuilder;
using NSSLServer.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NSSLServer.Models
{

    //public partial class EdekaProduct : IDatabaseProduct
    //{
    //    public string gtin { get; set; }
    //    public string addGtin { get; set; }
    //    public string name { get; set; }
    //    public string desc { get; set; }
    //    public string longdesc { get; set; }
    //    public string hinweis { get; set; }
    //    public string menge { get; set; }
    //    public string einheit { get; set; }
    //    public string region { get; set; }
    //    public long Id { get; set; }

    //    public Product convertToProduct()
    //    {
    //        List<string> eans = new List<string>();
    //        eans.AddRange(addGtin.Split(',').Where(s => s != "n/a"));
    //        eans.Add(gtin);
    //        return new Product
    //        {
    //            Name = desc,
    //            Codes = eans,
    //            Attributes = new Dictionary<string, string> {
    //            { "Internal name", name },
    //            { "Long description",longdesc},
    //            { "Note", hinweis},
    //            { "Quanitity", menge + einheit},
    //            { "Region", region}}
    //        };
    //    }
    //}

    public class EdekaProduct : IDatabaseProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal? Quantity { get; set; }
        public string LongDescription { get; set; }
        public string ShortDescription { get; set; }
        public virtual ICollection<EdekaGtinEntry> Gtins { get; set; }


        public static EdekaProductsTable EPT = new EdekaProductsTable("ept");
        public class EdekaProductsTable : Table<EdekaProductsTable>
        {
            public Field Id;
            public Field Name;
            public Field Quantity;
            public Field Unit;
            public Field LongDescription;
            public Field ShortDescription;

            public EdekaProductsTable(string alias = null) : base("edeka", "products", alias)
            {
                Id = F("id");
                Name = F("name");
                Quantity = F("quantity");
                Unit = F("unit");
                LongDescription = F("long_description");
                ShortDescription = F("short_description");
            }
        }

        public BasicProduct ConvertToProduct()=>
            new BasicProduct { Name = string.IsNullOrWhiteSpace(LongDescription) ? Name : LongDescription, Quantity = Quantity??0 , Unit = Unit??"", Gtin = Gtins?.Where(x=>x.Gtin.Length == 8 || x.Gtin.Length == 13).FirstOrDefault()?.Gtin };
        
        //public static implicit operator Product(EdekaProduct p)=>
        //    
    }
   

    public class EdekaRegion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public static RegionsTable ERT = new RegionsTable();

        public class RegionsTable : Table<RegionsTable>
        {
            public Field Id;
            public Field Name;
            public Field Key;

            public RegionsTable(string alias = null) : base("edeka", "regions", alias)
            {
                Id = F("id");
                Name = F("name");
                Key = F("key");
            }
        }
    }

    public class EdekaProductRegion
    {
        public int RegionId { get; set; }
        public int ProductId { get; set; }


        [ForeignKey(nameof(RegionId))]
        public virtual EdekaRegion Region { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual EdekaProduct Product { get; set; }

        public static ProductRegionsTable EPRT = new ProductRegionsTable();

        public class ProductRegionsTable : Table<ProductRegionsTable>
        {
            public Field RegionId;
            public Field ProductId;

            public ProductRegionsTable(string alias = null) : base("edeka", "product_regions", alias)
            {
                RegionId = F("region_id");
                ProductId = F("product_id");
            }
        }
    }
}
