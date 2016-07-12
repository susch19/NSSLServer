using Npgsql;
using System.Data.Common;
using System.Threading.Tasks;

namespace NSSLServer.Models.DatabaseConnection
{
    public static class DBConnection
    {
        public static async Task< DbConnection> OpenConnection()
        {
            return await NsslEnvironment.OpenConnectionAsync();
            //var con = new NpgsqlConnection("Server=127.0.0.1;Port=5432;Database=shoppinglist;User Id=shoppinglist;Password=shoppinglist;");
            //await con.OpenAsync();
            //return con;
        }
    }
}
