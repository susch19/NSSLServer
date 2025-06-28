using System.Collections.Generic;

namespace NSSL.Models
{
    public class ShoppingList
    {
        public List<ShoppingItem> Products { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            if (Products == null)
                return "";
            return $"{Id} {Name} {Products.Count} ";
        }
    }
}
