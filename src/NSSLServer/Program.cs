using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
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
            System.Threading.ThreadPool.SetMaxThreads(500, 500);
            Deviax.QueryBuilder.QueryExecutor.DefaultExecutor = new Deviax.QueryBuilder.PostgresExecutor();
            var host = new WebHostBuilder()
                .UseKestrel()
#if DEBUG
                .UseUrls("http://[::1]:4344", "http://192.168.49.28:4344")
#else
                .UseUrls("http://[::1]:4344", "http://127.0.0.1:4344")
#endif
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
