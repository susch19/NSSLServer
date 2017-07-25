using NSSLServer.Models;
using Npgsql;
using System.Data.Common;
using System.Linq;
using NSSLServer.Models.Products;
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

        public override void Dispose()
        {
            base.Dispose();
            if (_disposeConnection)
                Connection.Dispose();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Connection);
            
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var props = modelBuilder.Entity(entity.Name).Metadata.GetProperties().ToList();
                foreach (var prop in props)
                {
                    prop.Relational().ColumnName = string.Concat(prop.Name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
                }
            }
            modelBuilder.Entity<ShoppingList>()
                .HasMany(t => t.Products)
                .WithOne(a => a.ShoppingList)
                .IsRequired()
                .HasForeignKey(t => t.ListId);
            modelBuilder.Entity<ShoppingList>()
                .HasMany(t => t.Contributors)
                .WithOne(x=>x.ShoppingList)
                .HasForeignKey(t => t.ListId);
            modelBuilder.Entity<ShoppingList>()
                .ToTable("shoppinglists", "public")                
                .HasKey(t => t.Id);
                

            modelBuilder.Entity<User>()
                .HasMany(t => t.IsContributors)
                .WithOne(x=>x.User)
                .HasForeignKey(t => t.UserId);
            modelBuilder.Entity<User>()
                .HasMany(t => t.ShoppingLists);
                
            modelBuilder.Entity<User>()
                .ToTable("users", "public")
                .HasKey(t => t.Id);
            modelBuilder.Entity<Contributor>()
                .ToTable("contributors", "public")
                .HasKey(t => t.Id);
            modelBuilder.Entity<BasicProduct>().ToTable("products","public").HasKey(t => t.Gtin);
            modelBuilder.Entity<ListItem>()
                .ToTable("list_item", "public")
                .HasKey(t => t.Id);

        }
    }
}
