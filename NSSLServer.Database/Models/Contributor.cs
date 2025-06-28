using Deviax.QueryBuilder;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSSLServer.Models
{
    public class Contributor
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int UserId { get; set; }
        public short Permission { get; set; }

        public static class Permissions
        {
            public const short Owner = 10;
            public const short Admin = 5;
            public const short User = 1;

            public static bool CanChange(short user, short contributor) => user >= contributor && user >= Admin;
            public static bool IsAdmin(short s) => s == Admin;
        }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        [ForeignKey(nameof(ListId))]
        public virtual ShoppingList ShoppingList { get; set;}

        public static ContributorTable T = new ContributorTable("c");
        [PrimaryKey(nameof(Id))]
        public class ContributorTable : Table<ContributorTable>
        {
            public Field Id;
            public Field ListId;
            public Field UserId;
            public Field Permission;


            public ContributorTable(string tableAlias = null) : base("public","contributors", tableAlias)
            {
                Id = F("id");
                ListId = F("list_id");
                UserId = F("user_id");
                Permission = F("permission");
            }
        }
    }

}
