using Deviax.QueryBuilder;

using NLog;

using NSSLServer.Core.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
