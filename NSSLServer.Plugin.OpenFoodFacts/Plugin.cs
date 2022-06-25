using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Database;
using NSSLServer.Plugin.Products.Core;
using NSSLServer.Plugin.Shoppinglist.Sources;

using System;

namespace NSSLServer.Plugin.OpenFoodFacts
{
    public class Plugin : IPlugin
    {
        private Logger logger;

        public string Name => "OpenFood Facts API Plugin";

        public bool Initialize(LogFactory factory)
        {
            logger = factory.GetCurrentClassLogger();

            ProductSources.Instance.AddNewSource(new OpenFoodFactsSource());

            return true;
        }
    }
}
