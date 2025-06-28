using NLog;

using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.Products.Core
{
    public class ProductsPlugin : IPlugin
    {
        private Logger logger;

        public string Name { get; }

        public bool Initialize(LogFactory factory)
        {
            logger = factory.GetCurrentClassLogger();
            return true;
        }


    }
}
