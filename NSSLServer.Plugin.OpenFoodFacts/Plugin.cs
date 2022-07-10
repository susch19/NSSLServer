using Microsoft.Extensions.DependencyInjection;

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

        internal ServiceProvider ServiceProvider { get; private set; }

        public bool Initialize(LogFactory factory)
        {
            logger = factory.GetCurrentClassLogger();


            return true;
        }

        void IPlugin.ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            ServiceProvider = services.BuildServiceProvider();
            ProductSources.Instance.AddNewSource(ActivatorUtilities.CreateInstance<OpenFoodFactsSource>(ServiceProvider));
        }
    }
}
