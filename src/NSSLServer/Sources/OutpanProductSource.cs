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

namespace NSSLServer.Sources
{
    class OutpanProductSource : IProductSource
    {
        public bool islocal { get; } = false;

        async Task<BasicProduct> IProductSource.FindProductByCode(string code)
        {
            string url = "https://" + $"api.outpan.com/v2/products/{code}?apikey=1e0dea2842c3bd80559b9ef0a8df187b";
            var request = WebRequest.Create(url);
            Stream responseStream = (await  request.GetResponseAsync()).GetResponseStream();
            using (StreamReader sr = new StreamReader(responseStream))
            {
                JObject o = JObject.Parse(sr.ReadToEnd());
                var name = o["name"].ToString();
                var gtin = o["gtin"].ToString();

                if (name == null)
                    return null;

                LocalOutpanProductSource.AddProduct(name, gtin);

                BasicProduct p = new BasicProduct { Name = name, Gtin = gtin};
               
                return p;
            }
        }

        async Task<List<BasicProduct>> IProductSource.FindProductsByName(string name)
        {
            return new List<BasicProduct>();
        }
    }
}
