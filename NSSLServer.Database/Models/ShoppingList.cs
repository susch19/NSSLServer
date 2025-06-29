using Deviax.QueryBuilder;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSSLServer.Models
{
    [Table("shoppinglists")] //Should be migrated to shopping_lists
    public class ShoppingList
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        
        [InverseProperty(nameof(ListItem.ShoppingList))]
        public virtual List<ListItem> Products { get; set; }
        [InverseProperty(nameof(Contributor.ShoppingList))]
        public virtual List<Contributor> Contributors { get; set; }

        public ShoppingList() { }
        //public ShoppingList(User c, List<ListItem> p, string name)
        //{
        //    Products = p;
        //    //Contributors.Add(c);
        //    Name = name;
        //}

        public static ShoppingListTable T = new ShoppingListTable("sl");
        [PrimaryKey(nameof(Id))]
        public class ShoppingListTable : Table<ShoppingListTable>
        {
            public Field Id;
            public Field Name;

            public ShoppingListTable(string tableAlias = null) : base("public","shoppinglists" , tableAlias)
            {
                Id = F("id");
                Name = F("name");
            }
        }
    }

   
}