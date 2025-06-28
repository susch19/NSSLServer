using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using NSSLServer.Core.Extension;
using NSSLServer.Plugin.Products.Core;
using NSSLServer.Plugin.Shoppinglist.Sources;

namespace NSSLServer.Plugin.OpenFoodFacts;

public class Plugin : IPlugin
{
    /// <inheritdoc/>
    public string Name { get; } = "OpenFood Facts API Plugin";

    /// <inheritdoc/>
    public void Configure(WebApplication app)
    {
        ProductSources.Instance.AddNewSource(ActivatorUtilities.CreateInstance<OpenFoodFactsSource>(app.Services));
    }
}
