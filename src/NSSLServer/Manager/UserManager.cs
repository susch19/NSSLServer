﻿using Deviax.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NSSLServer.Models.User;
using NSSLServer.Models;
using System.Data.Common;
using System.Threading.Tasks;
using static Shared.ResultClasses;
using System.IO;
using Deviax.QueryBuilder.ChangeTracking;
using static NSSLServer.Features.PasswordRecovery;

namespace NSSLServer
{
    static class UserManager
    {
        /// <summary>
        /// WARNING RADIOACTIVE HAZARD
        /// <para>string = SEKRETKEY</para>
        /// <seealso cref="SecretKey"/>
        /// </summary>
#if true//youareadmin - lookinside
        #region DO NOT LOOK INSIDE HERE IS NOTHING TOO SEA
        public static byte[] SecretKey; //VERY VERY SECRET!
        #endregion  
#endif
        private static string mailUser; 
        private static string mailUserPwd;

        public static void ReadLoginInformation()
        {
            SecretKey = Encoding.UTF8.GetBytes(File.ReadAllText("secretkey"));
            mailUser = File.ReadAllLines(@"emailcert")[0];
            mailUserPwd = File.ReadAllLines(@"emailcert")[1];
        }

        public static async Task<CreateResult> CreateUser(string username, string email, string pwdhash)
        {
            using (var cont = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var exists = await FindUserByName(cont.Connection, username);
                if (exists != null)
                {
                    //await ChangePassword(exists.Id, "2", pwdhash);
                    return new CreateResult { Success = false, Error = "Username already taken" };
                }
                exists = await FindUserByEmail(cont.Connection, email);
                if (exists != null)
                    return new CreateResult { Success = false, Error = "Email already in use" };

                var minedsalt = GenerateSalt();
                var saltedpw = Salting(pwdhash, minedsalt);
                User c = new User(username.TrimEnd(), saltedpw, email.TrimEnd(), minedsalt);
                await Q.InsertOne(cont.Connection, c);

                cont.Connection.Close();
                return new CreateResult { Success = true, Id = c.Id, EMail = c.Email, Username = c.Username };
            }

        }

        public static async Task<Result> ChangePassword(int id, string o, string n, bool passwordReset = false)
        {
            using (var c = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var k = await Q.From(User.T).Where(x => x.Id.EqV(id)).FirstOrDefault<User>(c.Connection);// c.Users.FirstOrDefault(x => x.Id == id);
                if (k.PasswordHash.SequenceEqual(Salting(o, k.Salt)) || passwordReset)
                {
                    var ctc = ChangeTrackingContext.StartWith(k);
                    k.PasswordHash = Salting(n, k.Salt);
                    await ctc.Commit(c.Connection);
                }
                else
                {
                    c.Connection.Close();
                    return new Result { Success = false, Error = "old password was incorrect" };
                }
                c.Connection.Close();
                return new Result { Success = true };
            }
        }


        public static async Task<LoginResult> Login(string username, string email, string passwordhash)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                User exists = null;
                if (username != null)
                    exists = await FindUserByName(con.Connection, username);
                if (exists == null)
                {
                    if (email == null)
                        return new LoginResult { Success = false, Error = "user could not be found" };
                    exists = await FindUserByEmail(con.Connection, email);
                    if (exists == null)
                        return new LoginResult { Success = false, Error = "user could not be found" };
                }
                if (!Salting(passwordhash, exists.Salt).SequenceEqual(exists.PasswordHash))
                {
                    con.Connection.Close();
                    return new LoginResult { Success = false, Error = "password is incorrect" };
                }

                var payload = new Dictionary<string, object>()
                {
                    { "Expires", DateTime.UtcNow.AddMonths(1) },
                    { "Id", exists.Id},
                    {"Created", DateTime.UtcNow }
                };
                con.Connection.Close();
                return new LoginResult { Success = true, Error = "", Token = JsonWebToken.Encode(new Dictionary<string, object>(), payload, SecretKey, JsonWebToken.JwtHashAlgorithm.HS256), Id = exists.Id, EMail = exists.Email, Username = exists.Username };

            }
        }
        public static async Task<Result> SendPasswortResetEmail(string email)
        {
            using (var con = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                User exists = null;
                if (email != null)
                    exists = await FindUserByName(con.Connection, email);
                if (exists == null)
                {
                    exists = await FindUserByEmail(con.Connection, email);
                    if (exists == null)
                        return new Result { Success = false, Error = "user could not be found" };
                }

                var sender = new OutlookDotComMail(mailUser, mailUserPwd);
                var payload = new Dictionary<string, object>()
                {
                    { "Expires", DateTime.UtcNow.AddDays(1) },
                    { "Id", exists.Id },
                    { "Created", DateTime.UtcNow }
                };

                var token = JsonWebToken.Encode(new Dictionary<string, object>(), payload, SecretKey, JsonWebToken.JwtHashAlgorithm.HS256);
                var tokenUser = new TokenUserId(token, exists.Id);
                tokenUser.Timestamp = DateTime.UtcNow;
                await Q.InsertOne(con.Connection, tokenUser);
                sender.SendMail(exists.Email, "NSSL Password Reset",
                    $"Dear {exists.Username},\r\n\r\n" +
"This email was automatically sent following your request to reset your password.\r\n" +
"To reset your password, click this link or paste it into your browser's address bar:\r\n" +
"https://susch.undo.it/password/site/reset?token=" + token +
"\r\n\r\n" +
"If you did not forget your password, please ignore this email. Thank you.\r\n\r\n" +
"Kind Regards,\r\n" +
"NSSL Team");
                con.Connection.Close();
                return new Result { Success = true, Error = "" };

            }
        }
        public static async Task<Result> ResetPassword(string token, string n)
        {
            using (var c = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var rpt = await Q.From(TokenUserId.T).Where(x => x.Timestamp.GtV(DateTime.UtcNow.AddDays(-1)).And(x.ResetToken.EqV(token))).FirstOrDefault<TokenUserId>(c.Connection);

                if (rpt == null)
                    return new Result { Success = false, Error = "Token Expired or password reset was not requested" };
                var user = await Q.From(T).Where(x => x.Id.EqV(rpt.UserId)).FirstOrDefault<User>(c.Connection);
                if (user == null)
                    return new Result { Success = false, Error = "User for the token doesn't exists anymore" };
                await ChangePassword(user.Id, "", n, true);
                await Q.DeleteFrom(TokenUserId.T).Where(x => x.Timestamp.EqV(rpt.Timestamp).And(x.ResetToken.EqV(rpt.ResetToken).And(x.UserId.EqV(rpt.UserId)))).Execute(c.Connection);

                var sender = new OutlookDotComMail(mailUser, mailUserPwd);
                sender.SendMail(user.Email, "NSSL Password Reset",
$@"Dear {user.Username},

This email was sent to you, because you have successfully changed your password.


If it wasn't you, than this might be an indicator, that someone has access to your email account.


Kind Regards,
NSSL Team");
                c.Connection.Close();
                return new Result { Success = true };
            }
        }


        private static byte[] Salting(string passwordhash, byte[] salt)
        {
            var prov = System.Security.Cryptography.SHA512.Create();
            var hash = Encoding.UTF8.GetBytes(passwordhash).Concat(salt);
            return prov.ComputeHash(hash.ToArray());
        }
        private static byte[] GenerateSalt()
        {
            var num = System.Security.Cryptography.RandomNumberGenerator.Create();
            byte[] saltmine = new byte[128];
            num.GetBytes(saltmine);
            return saltmine;
        }

        public static async Task<User> FindUserByName(DbConnection con, string name) =>
             await Q.From(T).Where(T.Username.ILike(name)).FirstOrDefault<User>(con);

        public static async Task<User> FindUserByEmail(DbConnection con, string email) =>
             await Q.From(T).Where(T.Email.ILike(email)).FirstOrDefault<User>(con);

        public static async Task<User> FindUserById(DbConnection con, int id) =>
            await Q.From(T).Where(T.Id.EqV(id)).FirstOrDefault<User>(con);


        #region Unused code
        //public static async Task<UserInfo2> GetUserInfo(int id)
        //{
        //    var t2 = GetShoppingListsForUser(id);

        //    var t1 = Task.Run(async () =>
        //    {
        //        return await From(UT).Where(UT.Id.Eq(Q.P("id", id))).FirstOrDefault<User>(DBConnection.con);
        //    });

        //    await Task.WhenAll(t1);

        //    return new UserInfo2
        //    {
        //        Email = t1.Result.Email,
        //        Username = t1.Result.Username,
        //        Id = t1.Result.Id,
        //        ShoppingLists = t2
        //    };
        //}

        //public static List<ShoppingListInfo> GetShoppingListsForUser(DbConnection con, int id) =>
        //    Task.Run(async () =>
        //    {
        //            return await From(Contributor.CT)
        //                 .InnerJoin(ShoppingList.SLT).On(Contributor.CT.ListId.Eq(ShoppingList.SLT.Id))
        //                 .Select(ShoppingList.SLT.Id, ShoppingList.SLT.Name, Contributor.CT.IsAdmin)
        //                 .Where(Contributor.CT.UserId.Eq(Q.P("id", id))).ToList<ShoppingListInfo>(con);
        //    }).Result;

        //public static async Task<InfoUser> InfoAboutUser(string token)
        //{
        //    return await FindUserById((int)Authenticate(token)?["Id"]);
        //}
        #endregion
    }
}
