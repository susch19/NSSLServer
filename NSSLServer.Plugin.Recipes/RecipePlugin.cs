using NLog;
using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.Recipes
{
    public class RecipePlugin : IPlugin
    {
        public string Name => nameof(RecipePlugin);

        internal static Logger Logger { get; private set; }

        public bool Initialize(LogFactory factory)
        {
            Logger = factory.GetCurrentClassLogger();

            return true;
        }
    }
}