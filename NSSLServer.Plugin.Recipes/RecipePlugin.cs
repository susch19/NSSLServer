using NLog;
using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.Recipes
{
    public class RecipePlugin : IPlugin
    {
        public string Name => nameof(RecipePlugin);

        private Logger? logger;

        public bool Initialize(LogFactory factory)
        {
            logger = factory?.GetCurrentClassLogger();

            return true;
        }
    }
}