using System;

using NLog;

using NSSLServer.Core;
using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.InitializationHelper
{
    public class Plugin : IPlugin
    {
        public string Name { get; }

        public bool Initialize(LogFactory logFactory)
        {
            Console.WriteLine("Do you have Postresql installed? ((y)es, no)");
            Console.ReadLine();
            return false;

        }
    }
}
