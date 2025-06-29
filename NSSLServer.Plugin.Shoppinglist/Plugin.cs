using FirebaseAdmin;

using NLog;

using NSSLServer.Core.Extension;
using NSSLServer.Plugin.Products.Core;
using NSSLServer.Plugin.Shoppinglist.Manager;
using NSSLServer.Plugin.Shoppinglist.Sources;
using System.IO;
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

            Task.Run(async () => {
                using var c = new DBContext();
                using var oldC = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true);
                //await Task.Delay(2000);
                var list = await ShoppingListManager.LoadShoppingList(oldC, 28, false, 32);
                var listNew = await ShoppingListManagerNew.LoadShoppingList(c, 28, false, 32);
                //var listHist = await ShoppingListManager.LoadShoppingList(28, true, 32);
                //await ShoppingListManager.ChangeRights(c, 28, 32, 45);
                await ShoppingListManagerNew.ChangeListname(c, 28, 32, "Hering", "");
                var users = await ShoppingListManager.GetContributors(oldC, 28, 32);
                var usersNew = await ShoppingListManagerNew.GetContributors(c, 28, 32);
                //var alskdj = await ShoppingListManagerNew.AddList(c, "My New List", 32);

                var listsOld = await ShoppingListManager.LoadShoppingLists(oldC, 32);
                var listsNew = await ShoppingListManagerNew.LoadShoppingLists(c, 32);
                var eq = listsOld.Equals(listsNew);
                ;
            });
            return true;

        }
    }
}
