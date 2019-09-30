using NSSLServer.Models;
using Npgsql;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NSSLServer
{

    public class DBContext : DbContext
    {

        public DbSet<BasicProduct> Products { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ListItem> ShoppingItems { get; set; }
        public DbSet<Contributor> Contributors { get; set; }
        public DbSet<User> Users { get; set; }

        public DbConnection Connection;
        private bool _disposeConnection;


        public DBContext(DbConnection con, bool disposeConnection)
        {
            Connection = con;
            _disposeConnection = disposeConnection;
        }  
    }
}
