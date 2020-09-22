using FirebaseAdmin.Messaging;

using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Core.HelperMethods;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace NSSLServer.Features
{
    public class PluginLoader
    {
        public List<Type> ControllerTypes { get; } = new List<Type>();

        private List<IPlugin> plugins = new List<IPlugin>();
        private ILogger logger = LogManager.GetLogger(nameof(PluginLoader));

        internal void LoadPlugins(Assembly ass)
        {
            var allOfThemTypes = ass.GetTypes();

            foreach (var type in allOfThemTypes)
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    logger.Info($"Loading Plugin {type.Name} from Assembly {ass.FullName}");
                    plugins.Add(PluginCreator<IPlugin>.GetInstance(type));
                }
                if (typeof(BaseController).IsAssignableFrom(type))
                {
                    ControllerTypes.Add(type);
                }
            }
        }

        public void LoadAssemblies()
        {
            var paths = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");

            foreach (var path in paths)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                if (assembly.GetCustomAttribute<PluginAttribute>() is not null)
                {
                    logger.Info($"Loading Plugins from Assembly {assembly.FullName}");
                    LoadPlugins(assembly);
                }
            }
        }

        public void InitializePlugins(LogFactory logFactory)
        {
            foreach (var plugin in plugins)
            {
                if (!plugin.Initialize(logFactory))
                    logger.Warn($"Plugin {plugin.Name} had errors in initialization :(");
            }

        }
    }
}
