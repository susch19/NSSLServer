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
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NLog.Web;
using System.Reflection;

namespace NSSLServer
{
    public class Program
    {
        internal static PluginLoader PluginLoader;

        //dotnet restore
        //dotnet publish -r ubuntu.16.04-arm
        public static async Task Main(string[] args)
        {
            var logFactory = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
            System.Threading.ThreadPool.SetMaxThreads(500, 500);
            Deviax.QueryBuilder.QueryExecutor.DefaultExecutor = new Deviax.QueryBuilder.PostgresExecutor();
            PluginLoader = new PluginLoader();
            PluginLoader.LoadAssemblies();
            PluginLoader.InitializePlugins(logFactory);
            await PluginLoader.RunDbUpdates();


            var host = new WebHostBuilder()
                .UseKestrel()
#if DEBUG
                .UseUrls("http://[::1]:4344", "http://192.168.49.22:4344")
#else
                .UseUrls("http://+:80")
#endif
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseNLog()
                .UseStartup<Startup>()
                .Build();
            DoStuff();
            
            UserManager.ReadLoginInformation();

            host.Run();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async static void DoStuff()
        {
            FirebaseApp.Create(new AppOptions { Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("external/service_account.json")});

    
            //using (var c = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            //{
            //    c.Connection.Close();
            //    var asd = Q.From(ShoppingList.T).InnerJoin(ListItem.T).On((x, y) => x.Id.Eq(y.ListId)).ToList<ListItem>(c.Connection);
            //    //var query = Q.Create(ShoppingList.T)
            //    //    .Column(x => x.Id).Type("int").Null()
            //    //    .FK(ListItem.T).On((x, y) => x.Id.Eq(y.ListId)).On((x, y) => x.Name.Eq(y.Name))
            //    //    .Column(x => x.Name).Type("varchar(199)").NotNull()
            //    //    .Execute(c.Connection);
            //}
            //I like my do Stuff methods :)

      

        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    public static class FirebaseCloudMessaging
    {
        public static async Task TopicMessage<T>(string topicName, T payload)
        {
            var data = payload.GetType().GetProperties().ToDictionary(x => char.ToLowerInvariant(x.Name[0]) + x.Name.Substring(1), x => x.GetValue(payload).ToString());
            await FirebaseMessaging.DefaultInstance.SendAsync(new Message() { Topic = topicName, Data = data});
        }

        public static async Task TopicMessage(string topicName, string notificationTitle, string notificationBody)
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(new Message() { Topic = topicName, Notification = new Notification() { Title = notificationTitle, Body = notificationBody } });
        }
    }

}
