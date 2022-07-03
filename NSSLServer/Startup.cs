using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace NSSLServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            builder.AddEnvironmentVariables();
            builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var pluginWithControllerAssemblies = Program
                .PluginLoader
                .ControllerTypes
                .Select(x => x.Assembly)
                .Distinct()
                .ToArray();

            services
                .AddMvc()
                .ConfigureApplicationPartManager(manager =>
              {
                  foreach (var assembly in pluginWithControllerAssemblies)
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

            services.AddSwaggerGen(configuration =>
            {
                // Try get all documentation .xml files from plugins with controllers
                foreach (var pluginWithControllerAssembly in pluginWithControllerAssemblies)
                {
                    // Check plugin exists
                    var fileInfo = new FileInfo(pluginWithControllerAssembly.Location);
                    if (!fileInfo.Exists)
                        continue;

                    // Check documentation file exists
                    var file = fileInfo.FullName;
                    var docu = new FileInfo($"{file[..file.LastIndexOf(fileInfo.Extension)]}.xml");

                    if (!docu.Exists)
                        continue;

                    configuration.IncludeXmlComments(docu.FullName, true);
                }

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

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(configuration =>
            {
                configuration.SwaggerEndpoint("/swagger/v1/swagger.json", "NSSL API V1");
            });

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
