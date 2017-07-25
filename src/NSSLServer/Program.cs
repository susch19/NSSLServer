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
using System.Net;
using Firebase;
using Deviax.QueryBuilder;
using NSSLServer.Models;
using NSSLServer.Models.Products;
using static NSSLServer.Models.Products.GtinEntry;
using static NSSLServer.Models.Products.Product;
using static NSSLServer.Models.Products.ProductsGtins;
using static NSSLServer.ListItem;
using static NSSLServer.Models.Contributor;
using static NSSLServer.Models.User;
using static NSSLServer.Models.ShoppingList;

namespace NSSLServer
{
    public class Program
    {

        //dotnet restore
        //dotnet publish -r ubuntu.16.04-arm
        public static void Main(string[] args)
        {
            Deviax.QueryBuilder.QueryExecutor.DefaultExecutor = new Deviax.QueryBuilder.PostgresExecutor();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://[::1]:4344", "http://127.0.0.1:4344")
                .UseContentRoot(Directory.GetCurrentDirectory())               
                .UseStartup<Startup>()
                .Build();
            DoStuff();
            UserManager.ReadSecretKeyFromFile();

            host.Run();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async static void DoStuff()
        {
            //NSSLServer.Features.EdekaDatabaseUpdater.Initialize();
            //Registry.RegisterTypeToTable<EdekaProduct, EdekaProductsTable>();
            //Registry.RegisterTypeToTable<EdekaGtinEntry, GtinsTable>();
            Registry.RegisterTypeToTable<Product, ProductsTable>();
            Registry.RegisterTypeToTable<GtinEntry, GtinsTable>();
            Registry.RegisterTypeToTable<ProductsGtins, ProductsGtinsTable>();
            Registry.RegisterTypeToTable<ListItem, ListItemTable>();
            Registry.RegisterTypeToTable<Contributor, ContributorTable>();
            Registry.RegisterTypeToTable<User, UserTable>();
            Registry.RegisterTypeToTable<ShoppingList, ShoppingListTable>();
            //I like my do Stuff methods :)
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    }

    public static class FirebaseCloudMessaging
    {
        public static Firebase.FirebaseCloudMessaging fcm = new Firebase.FirebaseCloudMessaging(File.ReadAllText("firebase.key"));
    }

}
