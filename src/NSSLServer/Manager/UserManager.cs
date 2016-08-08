using Deviax.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NSSLServer.Models.User;
using static Deviax.QueryBuilder.Q;
using NSSLServer.Models;
using System.Data.Common;
using System.Threading.Tasks;
using static Shared.ResultClasses;

namespace NSSLServer
{
    static class UserManager
    {
        /// <summary>
        /// WARNING RADIOACTIVE HAZARD
        /// <para>string = SEKRETKEY</para>
        /// <seealso cref="SECRETKEY"/>
        /// </summary>
#if true//youareadmin - lookinside
        #region DO NOT LOOK INSIDE HERE IS NOTHING TOO SEA
        public static readonly byte[] SECRETKEY = Encoding.UTF8.GetBytes("LWehASIewSNDwiAPSohwe587║@;ÆY«⌂È47a56guUIw54qbekl9WI56376453▼Peou"); //VERY VERY SECRET!
        #endregion  
#endif
        public static async Task<CreateResult> CreateUser(string username, string email, string pwdhash)
        {
            using (var cont = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {

                var exists = await FindUserByName(cont.Connection, username);
                if (exists != null)
                    return new CreateResult { Success = false, Error = "Username already taken" };
                exists = await FindUserByEmail(cont.Connection, email);
                if (exists != null)
                    return new CreateResult { Success = false, Error = "Email already in use" };

                var minedsalt = generateSalt();
                var saltedpw = salting(pwdhash, minedsalt);
                User c = new User(username, saltedpw, email, minedsalt);

                cont.Users.Add(c);

                await cont.SaveChangesAsync();
                return new CreateResult { Success = true, Id = c.Id, EMail = c.Email, Username = c.Username };
            }

        }

        //public static string DeleteUser(DBContext con, )

        public static async Task<Result> ChangePassword(int id, string o, string n)
        {
            using (var c = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                var k = c.Users.FirstOrDefault(x => x.Id == id);
                if (k.PasswordHash.SequenceEqual(salting(o, k.Salt)))
                {
                    k.PasswordHash = salting(n, k.Salt);
                    await c.SaveChangesAsync();
                }
                else
                    return new Result { Success = false, Error = "old password was incorrect" };
                return new Result { Success = true };
            }
        }

        //public static IDictionary<string, object> Authenticate(string token)
        //{
        //    if (string.IsNullOrWhiteSpace(token))
        //        return null;
        //    try
        //    {
        //        var o = ((IDictionary<string, object>)JsonWebToken.DecodeToObject(token, SECRETKEY));
        //        var ex = ((DateTime)o["Expires"]).CompareTo(DateTime.UtcNow);
        //        if (((DateTime)o["Expires"]).CompareTo(DateTime.UtcNow) > 0)
        //            return o;
        //        else
        //            return null;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        //public static int GetIdFromToken(string token) =>
        //    (int)((IDictionary<string, object>)JsonWebToken.DecodeToObject(token, SECRETKEY))["Id"];


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
                if (!salting(passwordhash, exists.Salt).SequenceEqual(exists.PasswordHash))
                    return new LoginResult { Success = false, Error = "password is incorrect" };

                var payload = new Dictionary<string, object>()
            {
                { "Expires", DateTime.UtcNow.AddDays(1) },
                { "Id", exists.Id}
            };
                return new LoginResult { Success = true, Error = "", Token = JsonWebToken.Encode(new Dictionary<string, object>(), payload, SECRETKEY, JsonWebToken.JwtHashAlgorithm.HS512), Id = exists.Id, EMail = exists.Email, Username = exists.Username };

            }
        }

        private static byte[] salting(string passwordhash, byte[] salt)
        {
            var prov = System.Security.Cryptography.SHA512.Create();
            var hash = Encoding.UTF8.GetBytes(passwordhash).Concat(salt);
            return prov.ComputeHash(hash.ToArray());
        }
        private static byte[] generateSalt()
        {
            var num = System.Security.Cryptography.RandomNumberGenerator.Create();
            byte[] saltmine = new byte[128];
            num.GetBytes(saltmine);
            return saltmine;
        }

        public static async Task<User> FindUserByName(DbConnection con, string name) =>
             await From(UT).Where(UT.Username.Eq(P("sad", name.ToLower()))).FirstOrDefault<User>(con);

        public static async Task<User> FindUserByEmail(DbConnection con, string email) =>
             await From(UT).Where(UT.Email.Eq(P("sad", email.ToLower()))).FirstOrDefault<User>(con);

        public static async Task<User> FindUserById(DbConnection con, int id) => 
            await From(UT).Where(UT.Id.Eq(Q.P("id", id))).FirstOrDefault<User>(con);
        

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
