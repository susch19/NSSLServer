
using Deviax.QueryBuilder;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSSLServer.Models
{
    public class User
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public string Email { get; set; }

        [InverseProperty(nameof(Contributor.User))]
        public virtual ICollection<Contributor> IsContributors { get; set; }

        public User() { }
        public User(string name, byte[] pwdhash, string email, byte[] saltmine)
        {
            Username = name;
            PasswordHash = pwdhash;
            Email = email;
            Salt = saltmine;
        }
        
        public static readonly UserTable T = new UserTable("u");
        [PrimaryKey(nameof(Id))]
        public class UserTable : Table<UserTable>
        {
            public Field Id;
            public Field Username;
            public Field PasswordHash;
            public Field Salt;
            public Field Email;

            public UserTable(string alias = null) : base("public", "users", alias)
            {
                Id = F("id");
                Username = F("username");
                PasswordHash = F("password_hash");
                Email = F("email");
                Salt = F("salt");
            }
        }

        //TO String geht noch nicht
        //private static List<ShoppingListInfo> CreateShoppingListInfo(ref User u)
        //{
        //    var i = u.ShoppingLists?.Select(x=>new { x.Id,x.Name}).ToList();
            
        //    var z = u.IsContributors?.Select(x => x.IsAdmin).ToList();
        //    List<ShoppingListInfo> ret = new List<ShoppingListInfo>();
        //    for (int o = 0; o < i.Count; o++)
        //        ret.Add(new ShoppingListInfo { Id = i[o].Id, Name = i[o].Name, IsAdmin= z[o] });
        //    return ret;
        //}
        public static implicit operator Connection.User.UserInfo(User u) => new Connection.User.UserInfo { Id = u.Id.ToString(),Username = u.Username, Email = u.Email,
            ShoppingLists = null /*CreateShoppingListInfo(ref u)*/};

    }
}
