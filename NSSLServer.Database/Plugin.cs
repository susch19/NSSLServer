using Deviax.QueryBuilder;

using NLog;

using NSSLServer.Core.Extension;

namespace NSSLServer.Database
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "Database Core Plugin";

        public bool Initialize(LogFactory logFactory)
        {
            QueryExecutor.DefaultExecutor = new PostgresExecutor();

            return true;
        }
    }
}
