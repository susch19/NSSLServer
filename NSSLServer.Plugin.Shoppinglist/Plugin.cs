using FirebaseAdmin;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSSLServer.Core.Extension;
using NSSLServer.Plugin.Products.Core;
using NSSLServer.Plugin.Shoppinglist.Sources;
using System.IO;

namespace NSSLServer.Plugin.Shoppinglist;

public class Plugin : IPlugin
{
    /// <inheritdoc/>
    public string Name { get; } = "Plugin Shoppinglist";

    internal static ILogger Logger { get; private set; }

    /// <inheritdoc/>
    public void Configure(WebApplicationBuilder builder)
    {
        if (File.Exists("external/service_account.json"))
        {
            FirebaseApp.Create(new AppOptions { Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("external/service_account.json") });
        }

        ProductSources.Instance.AddNewSource(new ProductSource());
        ProductSources.Instance.AddNewSource(new OutpanProductSource());
    }

    /// <inheritdoc/>
    public void Configure(WebApplication app)
    {
        Logger = app.Services.GetRequiredService<ILogger<Plugin>>();
    }
}
