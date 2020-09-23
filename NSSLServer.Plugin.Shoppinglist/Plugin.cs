using FirebaseAdmin;

using NLog;

using NSSLServer.Core.Extension;

using System;
using System.Collections.Generic;
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
            FirebaseApp.Create(new AppOptions { Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("external/service_account.json") });
            return true;

        }
    }
}
