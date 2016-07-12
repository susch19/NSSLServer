using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSSLServer.Models;

namespace NSSLServer.Models.Connection.User
{
    public class UserInfo
    {
        public string Id;
        public string Username;
        public string Email;
        public List<ShoppingListInfo> ShoppingLists;
    }

    public class ShoppingListInfo
    {
        public int Id;
        public string Name;
        public bool IsAdmin;

        
    }
}
