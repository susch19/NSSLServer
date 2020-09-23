using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

using NSSLServer.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using NLog.Web;

namespace NSSLServer
{
    public class Program
    {
        internal static PluginLoader PluginLoader;

        public static async Task Main(string[] args)
        {
            var logFactory = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
            System.Threading.ThreadPool.SetMaxThreads(500, 500);

            PluginLoader = new PluginLoader();
            PluginLoader.LoadAssemblies();
            PluginLoader.InitializePlugins(logFactory);

            PluginLoader.InitializeDbUpdater();

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

            host.Run();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async static void DoStuff()
        {
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



}
