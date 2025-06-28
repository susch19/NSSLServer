using NSSLServer.Core.Extension;
using NSSLServer.Core.HelperMethods;
using NSSLServer.Database.Updater;
using System.Reflection;
using System.Runtime.Loader;

namespace NSSLServer.Features;

public class PluginLoader
{
    public List<Type> ControllerTypes { get; } = [];

    public List<IDbUpdater> DbUpdater { get; } = [];

    private readonly List<IPlugin> plugins = [];
    private readonly ILogger logger;

    private readonly Assembly assembly;
    private readonly string workdir;

    public PluginLoader(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        assembly = Assembly.GetExecutingAssembly();
        workdir = Path.GetDirectoryName(assembly.Location);
    }

    internal void LoadPlugins(Assembly ass)
    {
        var allOfThemTypes = ass.GetTypes();

        foreach (var type in allOfThemTypes)
        {
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                logger.LogInformation("Loading Plugin {typeName} from Assembly {assemblyName}", type.Name, ass.FullName);
                plugins.Add(PluginCreator<IPlugin>.GetInstance(type));
            }
            // TODO: each plugin should itself to the controllers
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

    /// <summary>
    /// Loads all assemblies and initializes the plugins.
    /// </summary>
    public void LoadAssemblies()
    {
        var pluginDir = Path.Combine(workdir, "plugins");

        if (Directory.Exists(pluginDir))
        {
            var toRemove = new FileInfo(Path.Combine(pluginDir, "ToRemove.txt"));
            if (toRemove.Exists)
            {
                foreach (var path in File.ReadAllLines(toRemove.FullName))
                {
                    logger.LogInformation("Deleting existing file {path}", path);
                    File.Delete(path);
                }
            }

            var allFiles = Directory.GetFiles(pluginDir);
            var plugins = allFiles.Where(x => x.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            var thirdParty = allFiles.Except(plugins);

            foreach (var plugin in plugins)
            {
                var filename = Path.GetFileName(plugin);
                logger.LogInformation("Copying Plugin Assembly {filename}", filename);

                File.Copy(plugin, Path.Combine(workdir, filename), true);
            }

            foreach (var plugin in thirdParty)
            {
                var filename = Path.GetFileName(plugin);
                logger.LogInformation("Copying Third Party File {filename}", filename);

                File.Copy(plugin, Path.Combine(workdir, filename), true);
            }
        }

        var paths = Directory.GetFiles(workdir, "*.dll");

        foreach (var path in paths)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            if (assembly.GetCustomAttribute<PluginAttribute>() is not null)
            {
                logger.LogInformation("Loading Plugins from Assembly {name}", assembly.FullName);
                LoadPlugins(assembly);
            }
        }
    }

    /// <summary>
    /// Configures the plugins during application startup.
    /// </summary>
    public void ConfigurePlugins(WebApplicationBuilder builder)
    {
        foreach (var plugin in plugins)
        {
            plugin.Configure(builder);
        }
    }

    /// <summary>
    /// Configures the plugins after all services are registered.
    /// </summary>
    public void ConfigurePlugins(WebApplication app)
    {
        foreach (var plugin in plugins)
        {
            plugin.Configure(app);
        }
    }
}
