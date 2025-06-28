using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Plugin.Userhandling.Manager;

namespace NSSLServer.Plugin.Userhandling
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "Plugin Userhandling";

        public bool Initialize(LogFactory logFactory)
        {
            UserManager.ReadLoginInformation();
            return true;
        }
    }
}
