using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

using NLog;

using NSSLServer.Core.Extension;

using System.IO;

namespace NSSLServer.Plugin.Webapp
{
    public class Plugin : IPlugin
    {
        public string Name => "WebappPlugin";

        internal static Logger Logger { get; private set; }

        public bool Initialize(LogFactory factory)
        {
            Logger = factory.GetCurrentClassLogger();

            return true;
        }

        void IPlugin.ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
        }

        void IPlugin.Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(environment.ContentRootPath, "Static", "web")),
                RequestPath = new PathString("/webapp")
            });
        }

    }
}