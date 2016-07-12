using Deviax.QueryBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User Owner { get; set; }

        public virtual ICollection<ListItem> Products { get; set; }
        public virtual ICollection<Contributor> Contributors { get; set; }
        public ShoppingList() { }
        public ShoppingList(User c, List<ListItem> p, string name)
        {
            Owner = c;
            Products = p;
            //Contributors.Add(c);
            Name = name;
        }

        public static ShoppingListTable SLT = new ShoppingListTable("sl");

        public class ShoppingListTable : Table<ShoppingListTable>
        {
            public Field Id;
            public Field Name;
            public Field UserId;

            public ShoppingListTable(string tableAlias = null) : base("public","shoppinglists" , tableAlias)
            {
                Id = F("id");
                Name = F("name");
                UserId = F("user_id");
            }
        }
    }

   
}