
using Deviax.QueryBuilder;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSSLServer.Models
{
    [Table("user_resettoken")]
    public class TokenUser
    {
        public string ResetToken { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }

        public TokenUser() { }
        public TokenUser(string token, int userId)
        {
            ResetToken = token;
            UserId = userId;
        }
        
        public static readonly TokenUserTable T = new TokenUserTable("tut");
        //[PrimaryKey(nameof(Reset))]
        public class TokenUserTable : Table<TokenUserTable>
        {
            public Field ResetToken;
            public Field UserId;
            public Field Timestamp;

            public TokenUserTable(string alias = null) : base("public", "user_resettoken", alias)
            {
                ResetToken = F("reset_token");
                UserId = F("user_id");
                Timestamp = F("timestamp");
               
            }
        }

    }
}
