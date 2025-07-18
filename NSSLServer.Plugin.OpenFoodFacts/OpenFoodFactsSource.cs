﻿using NSSLServer.Database;
using NSSLServer.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        private HttpClient _httpClient;

        public OpenFoodFactsSource(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        async Task<IDatabaseProduct?> IProductSource.FindProductByCode(string code)
        {
            if (cache.TryGetValue(code, out var product))
                return product;
            try
            {
                string url = @$"https://world.openfoodfacts.org/api/v0/product/{code}.json";
                var o = await _httpClient.GetFromJsonAsync<ProductSearchResult>(url);

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

                BasicProduct p = new BasicProduct { Name = $"{name} {brands}", Gtin = gtin, Quantity = quantity == 0m ? null : quantity, Unit = string.IsNullOrWhiteSpace(unit) ? null : unit };
                cache[code] = p;
                return p;
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<Paged<IDatabaseProduct>> IProductSource.FindProductsByName(string name, int i)
        {
            return new Paged<IDatabaseProduct>();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously


        private class ProductSearchResult
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


