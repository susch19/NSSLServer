using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

using NSSLServer.Core.Extension;

using System.IO;

namespace NSSLServer.Plugin.Webapp;

public class Plugin : IPlugin
{
    /// <inheritdoc/>
    public string Name { get; } = "WebappPlugin";

    /// <inheritdoc/>
    public void Configure(WebApplication app)
    {
        var path = Path.Combine(app.Environment.ContentRootPath, "Static", "web");
        Directory.CreateDirectory(path);

        app.UseFileServer(new FileServerOptions()
        {
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = new PathString("/webapp")
        });
    }
}