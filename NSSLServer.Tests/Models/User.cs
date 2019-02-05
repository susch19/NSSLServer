using NSSL.ServerCommunication;
using System;
using System.IO;
using System.Linq;

namespace NSSL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public string Email { get; set; }

        public static User GetUser(out bool tokenSuccess)
        {
            var testUserToken = Directory.GetFiles(Environment.CurrentDirectory, "test-user_*_token.txt").FirstOrDefault();

            tokenSuccess = testUserToken != null;

            var tokenInfo = File.ReadAllLines(testUserToken);
            HelperMethods.Token = tokenInfo[1];

            return GetUser();
        }
        public static User GetUser()
        {
            var testuser = Directory.GetFiles(Environment.CurrentDirectory, "test-user_*.txt").FirstOrDefault();
            if (testuser == null)
                return null;
            var userInfo = File.ReadAllLines(testuser);

            return new User
            {
                Id = int.Parse(userInfo[0]),
                Email = userInfo[1],
                Username = userInfo[2]
            };
        }
    }
}
