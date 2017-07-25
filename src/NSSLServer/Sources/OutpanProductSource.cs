using NSSLServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;

namespace NSSLServer.Sources
{
    class OutpanProductSource : IProductSource
    {
        public bool Islocal { get; } = false;
        public long Total { get; set; } = 0;

        //public async static Task AddProduct(string name, string gtin)
        //{
        //    // TODO Implement Adding Products to Outpan
        //}
        async Task<IDatabaseProduct> IProductSource.FindProductByCode(string code)
        {
            string url = "https://" + $"api.outpan.com/v2/products/{code}?apikey=1e0dea2842c3bd80559b9ef0a8df187b";
            var request = WebRequest.Create(url);
            Stream responseStream = (await  request.GetResponseAsync()).GetResponseStream();
            using (StreamReader sr = new StreamReader(responseStream))
            {
                JObject o = JObject.Parse(sr.ReadToEnd());
                var name = o["name"].ToString();
                var gtin = o["gtin"].ToString();

                if (string.IsNullOrWhiteSpace(name))
                    return null;

                //LocalCacheProductSource.AddProduct(name, gtin);

                BasicProduct p = new BasicProduct { Name = name, Gtin = gtin};
               
                return p;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<Paged<IDatabaseProduct>> IProductSource.FindProductsByName(string name, int i)
        {
            return new Paged<IDatabaseProduct>();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
