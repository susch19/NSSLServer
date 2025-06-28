using Microsoft.AspNetCore.Builder;

namespace NSSLServer.Core.Extension;

/// <summary>
/// Interface for plugins 
/// </summary>
/// <remarks>
///  Has to contain empty ctor, otherwise not loaded
/// </remarks>
public interface IPlugin
{
    /// <summary>
    /// The name of the plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Configures the plugin during application startup.
    /// </summary>
    public void Configure(WebApplicationBuilder builder) { }

    /// <summary>
    /// Configures the plugin after all services are registered.
    /// </summary>
    public void Configure(WebApplication app) { }
}
