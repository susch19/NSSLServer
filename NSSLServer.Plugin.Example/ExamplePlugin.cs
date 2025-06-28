using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.Example;

public class ExamplePlugin : IPlugin
{
    private ILogger<ExamplePlugin> logger;

    /// <inheritdoc/>
    public string Name { get; } = "Example Plugin";

    /// <inheritdoc/>
    public void Configure(WebApplicationBuilder builder)
    {
        // Register services here if needed.
    }

    /// <inheritdoc/>
    public void Configure(WebApplication app)
    {
        // Get services if needed.

        logger = app.Services.GetRequiredService<ILogger<ExamplePlugin>>();
        logger.LogInformation("Example Plugin loaded successfully.");
    }
}
