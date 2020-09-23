using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Plugin.Userhandling.Manager;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
