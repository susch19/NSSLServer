using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NSSLServer
{
    public class Program
    {


        public static void Main(string[] args)
        {
            Deviax.QueryBuilder.QueryExecutor.DefaultExecutor = new Deviax.QueryBuilder.PostgresExecutor();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:4344")
                .UseContentRoot(Directory.GetCurrentDirectory())               
                .UseStartup<Startup>()
                .Build();
            DoStuff();
            host.Run();
        }
        private async static void DoStuff()
        {
            var k = new Sources.EdekaProductSource();
            var z = await k.FindProductsByName("nudel soße",1);
            var z1 = await k.FindProductsByName("käse",2);


        }
        
    }

}
