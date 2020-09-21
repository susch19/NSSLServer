
using Deviax.QueryBuilder;
using NSSLServer.Models.Connection.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Models
{
    public class TokenUserId
    {
        public string ResetToken { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }

        public TokenUserId() { }
        public TokenUserId(string token, int userId)
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
