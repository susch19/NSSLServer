using FirebaseAdmin;

using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Core.HelperMethods;
using NSSLServer.Database;
using NSSLServer.Models;
using NSSLServer.Plugin.Products.Core;
using NSSLServer.Plugin.Shoppinglist.Sources;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Plugin.Shoppinglist
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "Plugin Shoppinglist";
        public static Logger Logger { get; private set; }

        public bool Initialize(LogFactory logFactory)
        {
            Logger = logFactory.GetCurrentClassLogger();
            if (File.Exists("external/service_account.json"))
            {
                FirebaseApp.Create(new AppOptions { Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("external/service_account.json") });
            }

            ProductSources.Instance.AddNewSource(new ProductSource());
            ProductSources.Instance.AddNewSource(new OutpanProductSource());
            return true;

        }
    }
}
