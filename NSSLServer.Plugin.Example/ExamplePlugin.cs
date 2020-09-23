using NLog;

using NSSLServer.Core.Extension;

using System;

namespace NSSLServer.Plugin.Example
{
    public class ExamplePlugin : IPlugin
    {
        private Logger logger;

        public string Name { get; }

        public bool Initialize(LogFactory factory)
        {
            logger = factory.GetCurrentClassLogger();
            return true;
        }


    }
}
