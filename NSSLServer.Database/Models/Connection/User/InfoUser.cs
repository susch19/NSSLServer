using System.Collections.Generic;

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
