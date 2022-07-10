using FirebaseAdmin.Messaging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Core.HelperMethods;
using NSSLServer.Database.Updater;

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
        public List<IDbUpdater> DbUpdater { get; } = new List<IDbUpdater>();

        private List<IPlugin> plugins = new List<IPlugin>();
        private readonly ILogger logger;

        public PluginLoader(LogFactory logFactory)
        {
            logger = logFactory.GetCurrentClassLogger();
        }

        internal void LoadPlugins(Assembly ass)
        {
            var allOfThemTypes = ass.GetTypes();

            foreach (var type in allOfThemTypes)
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    logger.Info($"Loading Plugin {type.Name} from Assembly {ass.FullName}");
                    plugins.Add(PluginCreator<IPlugin>.GetInstance(type));
                }
                else if (typeof(BaseController).IsAssignableFrom(type))
                {
                    ControllerTypes.Add(type);
                }
                else if (typeof(IDbUpdater).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    var updater = PluginCreator<IDbUpdater>.GetInstance(type);
                    DbUpdater.Add(updater);
                }

            }
        }

        internal void InitializeConfigure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            foreach (var item in plugins)
                item.Configure(app, environment);

        }

        internal void InitializeConfigureServices(IServiceCollection services)
        {
            foreach (var item in plugins)
                item.ConfigureServices(services);

        }

        internal void InitializeDbUpdater()
        {
            var updaterLoadTasks = new List<Task>();
            foreach (var updater in DbUpdater)
            {
                updater.LoadDesiredVersion();
                updaterLoadTasks.Add(updater.LoadCurrentVersion());
                updater.RegisterTypes();
            }

            foreach (var loadTasks in updaterLoadTasks)
            {

                if (!loadTasks.IsCompleted)
                {
                    loadTasks.Wait();
                    loadTasks.Dispose();
                }
            }
        }

        internal async Task RunDbUpdates()
        {
            foreach (var updater in DbUpdater.OrderBy(x => x.Priority))
            {
                try
                {
                    await updater.RunUpdates();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void LoadAssemblies()
        {
            var workdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (Directory.Exists(Path.Combine(workdir, "plugins")))
            {
                var toRemove = new FileInfo(Path.Combine(workdir, "plugins", "ToRemove.txt"));
                if (toRemove.Exists)
                {
                    foreach (var path in File.ReadAllLines(toRemove.FullName))
                    {
                        logger.Info($"Deleting existing file {path}");
                        File.Delete(path);
                    }
                }

                var plugins = Directory.GetFiles(Path.Combine(workdir, "plugins"), "*.dll");
                foreach (var plugin in plugins)
                {
                    var filename = Path.GetFileName(plugin);
                    logger.Info($"Copying Plugin Assembly {filename}");

                    File.Copy(plugin, Path.Combine(workdir, filename), true);
                }

                var thirdParty = Directory
                    .GetFiles(Path.Combine(workdir, "plugins"))
                    .Where(x => !x.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase));

                foreach (var plugin in thirdParty)
                {
                    var filename = Path.GetFileName(plugin);
                    logger.Info($"Copying Third Party File {filename}");

                    File.Copy(plugin, Path.Combine(workdir, filename), true);
                }
            }


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
