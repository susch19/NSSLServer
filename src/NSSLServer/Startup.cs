using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace NSSLServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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
            app.UseMvc();
        }
    }
}
