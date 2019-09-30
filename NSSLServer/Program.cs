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
using NSSLServer.Features;
using static NSSLServer.Features.PasswordRecovery;
using static NSSLServer.Models.TokenUserId;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

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
                .UseUrls("http://+:80")
#endif
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseStartup<Startup>()
                .Build();
            DoStuff();
            UserManager.ReadLoginInformation();

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
            Registry.RegisterTypeToTable<TokenUserId, TokenUserTable>();
            using (var c = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                c.Connection.Close();
            }
            //I like my do Stuff methods :)
            EdekaDatabaseUpdater.Initialize();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    public static class FirebaseCloudMessaging
    {
        public static Firebase.FirebaseCloudMessaging fcm = new Firebase.FirebaseCloudMessaging(File.ReadAllText("external/firebase.key"));
    }

}
