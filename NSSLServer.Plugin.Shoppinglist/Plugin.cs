using FirebaseAdmin;

using NLog;

using NSSLServer.Core.Extension;

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

        public bool Initialize(LogFactory logFactory)
        {
            if (File.Exists("external/service_account.json"))
            {
                FirebaseApp.Create(new AppOptions { Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("external/service_account.json") });
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
