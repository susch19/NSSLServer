using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NSSLServer.Database;
using NSSLServer.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NSSLServer.Plugin.Shoppinglist.Sources
{
    public class OpenFoodFactsSource : IProductSource
    {
        private Dictionary<string, BasicProduct> cache = new();

        public bool Islocal { get; } = false;
        public long Total { get; set; } = 0;
        public int Priority { get; } = 10;

        private Regex regex = new Regex("[^a-zA-Z -]");

        //public async static Task AddProduct(string name, string gtin)
        //{
        //    // TODO Implement Adding Products to Outpan
        //}
        async Task<IDatabaseProduct?> IProductSource.FindProductByCode(string code)
        {
            if (cache.TryGetValue(code, out var product))
                return product;

            string url = @$"https://world.openfoodfacts.org/api/v0/product/{code}.json";
            var request = WebRequest.Create(url);
            var res = await request.GetResponseAsync();
            Stream responseStream = res.GetResponseStream();
            using StreamReader sr = new StreamReader(responseStream);

            var o = JsonConvert.DeserializeObject<Rootobject>(sr.ReadToEnd());

            if (o.Status != 1)
                return null;

            var name = o.Product.Product_name;
            var brands = o.Product.Brands;
            if (!string.IsNullOrWhiteSpace(brands) && brands.Contains(","))
                brands = brands.Split(",").First();

            var gtin = o.Code;
            var quant = o.Product.Product_quantity;
            var unit = o.Product.Quantity; //extract g, ml, etc. pp.

            if (string.IsNullOrWhiteSpace(name))
                return null;

            decimal.TryParse(quant, out var quantity);

            if (!string.IsNullOrWhiteSpace(unit))
            {
                var newUnit = regex.Replace(unit, "");
                var quantityForUnit = unit.Replace(newUnit, "");
                unit = newUnit;

                if (decimal.TryParse(quantityForUnit, out var quantForUnit))
                    quantity = quantForUnit;
            }

            BasicProduct p = new BasicProduct { Name = $"{name} {brands}" , Gtin = gtin, Quantity = quantity, Unit = unit };
            cache[code] = p;
            return p;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<Paged<IDatabaseProduct>> IProductSource.FindProductsByName(string name, int i)
        {
            return new Paged<IDatabaseProduct>();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously


        private class Rootobject
        {
            public string Code { get; set; }
            public Product? Product { get; set; }
            public int Status { get; set; }
            public string Status_verbose { get; set; }
        }

        private class Product
        {
            public string? Brand_owner { get; set; }
            public string? Brand_owner_imported { get; set; }
            public string? Brands { get; set; }
            public string? Code { get; set; }
            public string? Generic_name { get; set; }
            public string? Generic_name_en { get; set; }
            public string? Id { get; set; }
            public string? Product_name { get; set; }
            public string? Product_name_en { get; set; }
            public string? Product_quantity { get; set; }
            public string? Purchase_places { get; set; }
            public string? Quantity { get; set; }
        }

    }
}


