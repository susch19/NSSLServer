
using Deviax.QueryBuilder;

using NSSLServer.Database.Models;
using NSSLServer.Models;
using NSSLServer.Models.Products;


namespace NSSLServer.Database.Updater
{
    public class FrameworkDbUpdater : DbUpdater
    {
        public override int Priority { get; } = 1;

        public override void RegisterTypes()
        {
            Registry.RegisterTypeToTable<Product, Product.ProductsTable>();
            Registry.RegisterTypeToTable<GtinEntry, GtinEntry.GtinsTable>();
            Registry.RegisterTypeToTable<ProductsGtins,ProductsGtins. ProductsGtinsTable>();
            Registry.RegisterTypeToTable<ListItem, ListItem.ListItemTable>();
            Registry.RegisterTypeToTable<Contributor, Contributor.ContributorTable>();
            Registry.RegisterTypeToTable<User, User.UserTable>();
            Registry.RegisterTypeToTable<ShoppingList, ShoppingList.ShoppingListTable>();
            Registry.RegisterTypeToTable<TokenUser, TokenUser.TokenUserTable>();
            Registry.RegisterTypeToTable<DbVersion, DbVersion.DbVersionTable>();
        }
    }
}
