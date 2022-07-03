using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

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
                  foreach (var assembly in Program.PluginLoader.ControllerTypes.Select(x => x.Assembly).Distinct())
                  {
                      var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                      foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
                      {
                          manager.ApplicationParts.Add(applicationPart);
                      }
                  }
              });
            services.AddControllers().AddNewtonsoftJson();
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.Configure<GzipCompressionProviderOptions>(conf => conf.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
            services.AddHttpClient();

#if DEBUG
            services.AddSwaggerGen(configuration =>
            {
                configuration.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "NSSLServer.xml"));
                configuration.SwaggerDoc("v1", new OpenApiInfo { Title = "NSSL API", Version = "v1" });
                configuration.AddSecurityDefinition("X-Token", new OpenApiSecurityScheme()
                {
                    Name = "X-Token",
                    Scheme = "string",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                });

                configuration.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Id = "X-Token",
                                Type = ReferenceType.SecurityScheme,
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
#endif

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //#if DEBUG
            //            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //            loggerFactory.AddDebug();
            //#endif

//#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(configuration =>
            {
                configuration.SwaggerEndpoint("/swagger/v1/swagger.json", "NSSL API V1");
            });
//#endif

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseRouting();
            app.Use(async (ctx, f) =>
            {
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
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}");
            });
        }
    }
}
