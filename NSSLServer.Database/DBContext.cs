using NSSLServer.Models;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace NSSLServer
{

    public class DBContext : DbContext
    {

        public DbSet<BasicProduct> Products { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ListItem> ListItems { get; set; }
        public DbSet<Contributor> Contributors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TokenUser> TokenUser { get; set; }
        
        public DbConnection Connection;
        private bool _disposeConnection;

        public DBContext()
        {

        }

        public DBContext(DbConnection con, bool disposeConnection)
        {
            Connection = con;
            _disposeConnection = disposeConnection;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BasicProduct>().HasNoKey();
            //modelBuilder.Entity<User>();
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseNpgsql(NsslEnvironment.ConnectionString)
                .UseSnakeCaseNamingConvention();

            base.OnConfiguring(optionsBuilder);
        }
    }
}
