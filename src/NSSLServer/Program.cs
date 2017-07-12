using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using Firebase;

namespace NSSLServer
{
    public class Program
    {

        //dotnet restore
        //dotnet publish -r ubuntu.16.04-arm
        public static void Main(string[] args)
        {
            Deviax.QueryBuilder.QueryExecutor.DefaultExecutor = new Deviax.QueryBuilder.PostgresExecutor();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://[::1]:4344", "http://127.0.0.1:4344")
                .UseContentRoot(Directory.GetCurrentDirectory())               
                .UseStartup<Startup>()
                .Build();
            DoStuff();
            UserManager.ReadSecretKeyFromFile();

            host.Run();
        }
        private async static void DoStuff()
        {

        }
        
    }

    public static class FirebaseCloudMessaging
    {
        public static Firebase.FirebaseCloudMessaging fcm = new Firebase.FirebaseCloudMessaging(File.ReadAllText("firebase.key"));
    }

}
