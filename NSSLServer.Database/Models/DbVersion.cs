using Deviax.QueryBuilder;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSSLServer.Database.Models
{
    [Table("dbversion")]
    public class DbVersion
    {
        public string Name { get; set; }
        public string Version { get; set; }


        public static DbVersionTable T = new DbVersionTable("dbversion");
        [PrimaryKey(nameof(Name))]
        public class DbVersionTable : Table<DbVersionTable>
        {
            public Field Name;
            public Field Version;

            public DbVersionTable(string tableAlias = null) : base("public", "dbversion", tableAlias)
            {
             
                Name = F("name");
                Version = F("version");
          
            }
        }
    }
}
