using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using NSSLServer.Features;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NSSLServer
{
    public class Program
    {
        internal static PluginLoader PluginLoader;

#if DEBUG
        private const int Port = 4344;
#else
        private const int Port = 80;
#endif

        public static async Task Main(string[] args)
        {
            var logFactory = NLogBuilder.ConfigureNLog("nlog.config");
            ThreadPool.SetMaxThreads(500, 500);

            PluginLoader = new PluginLoader(logFactory);
            PluginLoader.LoadAssemblies();

            PluginLoader.InitializePlugins(logFactory);

            PluginLoader.InitializeDbUpdater();

            await PluginLoader.RunDbUpdates();

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                Args = args,
                ContentRootPath = Directory.GetCurrentDirectory(),
            });

            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.WebHost.UseKestrel(ks => ks.ListenAnyIP(Port));
            builder.WebHost.UseNLog();

            var startup = new Startup(builder.Configuration, builder.Environment);
            startup.ConfigureServices(builder.Services);
            PluginLoader.InitializeConfigureServices(builder.Services);

            var app = builder.Build();
            startup.Configure(app, app.Environment);
            PluginLoader.InitializeConfigure(app, app.Environment);
            app.Run();
            
        }
    }
}
