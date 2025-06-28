using Deviax.QueryBuilder;
using Microsoft.AspNetCore.Builder;

using NSSLServer.Core.Extension;

namespace NSSLServer.Database;

public class Plugin : IPlugin
{
    /// <inheritdoc/>
    public string Name { get; } = "Database Core Plugin";

    /// <inheritdoc/>
    public void Configure(WebApplication app)
    {
        QueryExecutor.DefaultExecutor = new PostgresExecutor();
    }
}
