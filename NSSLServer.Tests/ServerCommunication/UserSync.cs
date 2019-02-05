using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static NSSL.ServerCommunication.HelperMethods;
using static Shared.RequestClasses;
using static Shared.ResultClasses;

namespace NSSLServer.Tests.ServerCommunication
{
    public static class UserSync
    {
        private static string path = "users";


        public static async Task<CreateResult> Create(string username, string email, string password)
        =>  await PostAsync<CreateResult>("registration", new LoginArgs {Username = username , PWHash = password, EMail = email });

        public static async Task<LoginResult> Login(string username, string password)
        => await PostAsync<LoginResult>($"session", new LoginArgs { Username = username, PWHash = password });

        public static async Task<LoginResult> LoginEmail(string email, string password)
        => await PostAsync<LoginResult>($"session", new LoginArgs {EMail = email, PWHash = password });
        
        public static async Task<InfoResult> Info()
        => await GetAsync<InfoResult>($"{path}");

        public static async Task<SessionRefreshResult> RefreshToken()
        => await PutAsync<SessionRefreshResult>("session");

        public static async Task<Result> ChangePassword(int userId, string oldPassword, string newPassword, string token)
        => await PutAsync<Result>($"{path}", new ChangePasswordArgs {OldPWHash = oldPassword, NewPWHash = newPassword});

        public static bool DeleteUser()
        {
            return false;
        }
    }
}
