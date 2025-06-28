using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using NSSLServer.Features;
using Serilog;

ThreadPool.SetMaxThreads(500, 500);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.File("external/logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

using var factory = new Serilog.Extensions.Logging.SerilogLoggerFactory();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
});

builder.Services.AddSerilog();

var PluginLoader = new PluginLoader(factory.CreateLogger<ILogger<PluginLoader>>());
PluginLoader.LoadAssemblies();
PluginLoader.InitializeDbUpdater();

await PluginLoader.RunDbUpdates();

builder.Configuration
.SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Configuration.AddEnvironmentVariables();

var pluginWithControllerAssemblies = PluginLoader
    .ControllerTypes
    .Select(x => x.Assembly)
    .Distinct()
    .ToArray();

var services = builder.Services;
services.AddCors();
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

PluginLoader.ConfigurePlugins(builder);

var app = builder.Build();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
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
    ctx.Response.Headers.AccessControlAllowOrigin = "*";// ctx.Request.Headers.TryGetValue("Origin", out StringValues originValues) ? originValues[0] : "*";
    ctx.Response.Headers.AccessControlAllowCredentials = "true";
    if (ctx.Request.Method == "OPTIONS")
    {
        ctx.Response.Headers.AccessControlAllowMethods = "GET, POST, PUT, DELETE";
        ctx.Response.Headers.AccessControlAllowHeaders = "Origin, X-Token, X-Requested-With, Content-Type, Accept";
        ctx.Response.Headers.AccessControlMaxAge = "1728000";
    }
    else
    {
        await f();
    }
});

app.MapControllerRoute("default", "{controller=Home}/{action=Index}");

PluginLoader.ConfigurePlugins(app);
app.Run();

Log.CloseAndFlush();
