using Microsoft.AspNetCore.Builder;

using NSSLServer.Core.Extension;
using NSSLServer.Plugin.Userhandling.Manager;

namespace NSSLServer.Plugin.Userhandling;

public class Plugin : IPlugin
{
    /// <inheritdoc/>
    public string Name { get; } = "Plugin Userhandling";

    /// <inheritdoc/>
    public void Configure(WebApplicationBuilder builder)
    {
        UserManager.ReadLoginInformation();
    }
}
