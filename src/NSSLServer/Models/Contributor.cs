using Deviax.QueryBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public class Contributor
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        [ForeignKey(nameof(ListId))]
        public virtual ShoppingList ShoppingList { get; set;}

        public static ContributorTable CT = new ContributorTable("c");

        public class ContributorTable : Table<ContributorTable>
        {
            public Field Id;
            public Field ListId;
            public Field UserId;
            public Field IsAdmin;


            public ContributorTable(string tableAlias = null) : base("public","contributors", tableAlias)
            {
                Id = F("id");
                ListId = F("list_id");
                UserId = F("user_id");
                IsAdmin = F("is_admin");
            }
        }
    }

}
