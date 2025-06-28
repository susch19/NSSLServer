using System;
using System.IO;

using NLog;
using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.InitializationHelper
{
    public class Plugin : IPlugin
    {
        public string Name { get; }

        public bool Initialize(LogFactory logFactory)
        {
            AskForPostgres();

            while (!GetConnectionString()) { };

            GetSecretKeyForJwt();

            CreateServiceAccountForFirebase();

            GetEmailCert();

            return true;
        }

        private void GetEmailCert()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "external", "emailcert");
            if (File.Exists(filePath))
                return;
            File.WriteAllLines(filePath, new[] { "test", "test" });
        }

        private void CreateServiceAccountForFirebase()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "external", "service_account.json")))
                return;
            Console.WriteLine("You can create a firebase account, for firebase messaging. This service is free by google and normaly required for the shoppinglist plugin.");
            Console.WriteLine("To do so, you can visit the firebase url and follow the stepts there: https://firebase.google.com/products/cloud-messaging");
            Console.WriteLine("After you've created everything, you should be able to download a 'service_account.json', please copy this file into the external folder, where the secretkey and connectionstring are at.");
            Console.WriteLine("Path should be: " + Path.Combine(Directory.GetCurrentDirectory(), "external"));
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private bool GetSecretKeyForJwt()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "external", "secretkey")))
                return true;

            Console.Clear();
            Console.WriteLine("Please insert a random string of letters for the jwt key, required for login token generation.");
            Console.WriteLine("Can be changed afterwards in the 'secretkey' file in the external folder. After changing it, the current login tokens will be invalidated.");

            var jwtSecret = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(jwtSecret))
                return false;
            Directory.CreateDirectory("external");
            File.WriteAllText(Path.Combine("external", "secretkey"), jwtSecret);
            return true;
        }

        private bool GetConnectionString()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "external", "connectionstring")))
                return true;
            Console.Clear();
            Console.WriteLine("Please insert you connection string for the database.");
            Console.WriteLine("Example: User Id=postgres;Server=127.0.0.1;Port=5432;Password=password;Database=testInit;Pooling=true;Minimum Pool Size=10;Trust Server Certificate=True;");
            Console.WriteLine("If you want to change the connectionstring afterwards, you have to edit the 'connectionstring' file in the external folder");

            var conString = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(conString))
                return false;
            Directory.CreateDirectory("external");
            File.WriteAllText(Path.Combine("external", "connectionstring"), conString);
            return true;
        }

        private bool AskForPostgres()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "external", "connectionstring")))
                return true;
            Console.WriteLine("Do you have Postresql installed? ((y)es, no)");
            var res = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(res) || res.ToLower().StartsWith("n"))
            {
                Console.Clear();
                Console.WriteLine("Please install Postgres first. You can download it from https://www.postgresql.org/download/");
                Console.WriteLine("If you have installed it, press any key");
                Console.ReadKey();
                res = null;
            }
            return string.IsNullOrWhiteSpace(res) || res.ToLower().StartsWith("y");
        }
    }
}
