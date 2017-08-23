using Deviax.QueryBuilder;
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

        public static void ReadSecretKeyFromFile()
        {
            SecretKey = Encoding.UTF8.GetBytes(File.ReadAllText("secretkey"));
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

                return new CreateResult { Success = true, Id = c.Id, EMail = c.Email, Username = c.Username };
            }

        }


        public static async Task<Result> ChangePassword(int id, string o, string n)
        {
            using (var c = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var k = await Q.From(User.T).Where(x => x.Id.EqV(id)).FirstOrDefault<User>(c.Connection);// c.Users.FirstOrDefault(x => x.Id == id);
                if (k.PasswordHash.SequenceEqual(Salting(o, k.Salt)))
                {
                    var ctc = ChangeTrackingContext.StartWith(k);
                    k.PasswordHash = Salting(n, k.Salt);
                    await ctc.Commit(c.Connection);
                }
                else
                    return new Result { Success = false, Error = "old password was incorrect" };
                return new Result { Success = true };
            }
        }


        public static async Task<LoginResult> Login(string username, string email, string passwordhash)
        {
            using (var con = await NsslEnvironment.OpenConnectionAsync())
            {
                User exists = null;
                if (username != null)
                    exists = await FindUserByName(con, username);
                if (exists == null)
                {
                    if (email == null)
                        return new LoginResult { Success = false, Error = "user could not be found" };
                    exists = await FindUserByEmail(con, email);
                    if (exists == null)
                        return new LoginResult { Success = false, Error = "user could not be found" };
                }
                if (!Salting(passwordhash, exists.Salt).SequenceEqual(exists.PasswordHash))
                    return new LoginResult { Success = false, Error = "password is incorrect" };

                var payload = new Dictionary<string, object>()
                {
                    { "Expires", DateTime.UtcNow.AddMonths(1) },
                    { "Id", exists.Id},
                    {"Created", DateTime.UtcNow }
                };
                return new LoginResult { Success = true, Error = "", Token = JsonWebToken.Encode(new Dictionary<string, object>(), payload, SecretKey, JsonWebToken.JwtHashAlgorithm.HS256), Id = exists.Id, EMail = exists.Email, Username = exists.Username };

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
