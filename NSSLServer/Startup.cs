using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

using NSSLServer.Features;

namespace NSSLServer
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //if (env.IsEnvironment("Development"))
            //{
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                //builder.AddApplicationInsightsSettings(developerMode: true);
            //}

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);
            services
                .AddMvc()
                .ConfigureApplicationPartManager(manager =>
              {
                  manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
              });
            services.AddControllers().AddNewtonsoftJson();
            services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.Configure<GzipCompressionProviderOptions>(conf => conf.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app)
        {
//#if DEBUG
//            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
//            loggerFactory.AddDebug();
//#endif
            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseRouting();
            app.Use(async (ctx, f) => {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = ctx.Request.Headers.TryGetValue("Origin", out StringValues originValues) ? originValues[0] : "*";
                ctx.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                if (ctx.Request.Method == "OPTIONS")
                {
                    ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE";
                    ctx.Response.Headers["Access-Control-Allow-Headers"] = "Origin, X-Token, X-Requested-With, Content-Type, Accept";
                    ctx.Response.Headers["Access-Control-Max-Age"] = "1728000";
                }
                else
                {
                    await f();
                }
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
