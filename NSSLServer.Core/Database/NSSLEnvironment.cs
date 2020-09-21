using Npgsql;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NSSLServer
{
    public class NsslEnvironment
    {
        public static string ConnectionString = File.ReadAllText("external/connectionstring"); //+ "Connection Idle Lifetime=60;Maximum Pool Size=1024"; 

        public static async Task<DbConnection> OpenConnectionAsync()
        {

            var con = new NpgsqlConnection(ConnectionString);
            await con.OpenAsync();//..ConfigureAwait(false);

            return con;
        }
    }
}
