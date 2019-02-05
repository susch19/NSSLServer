using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSSL.ServerCommunication
{
    public static class HelperMethods
    {
        #region url
        public static string url = "https://susch.undo.it";
       // public static string url = "http://192.168.49.28:4344";

        #endregion
        public static string Token;


        public static async Task<T> GetAsync<T>(string requestPath)
            => await WithoutBody<T>(requestPath, "GET");
        public static async Task<T> PostAsync<T>(string requestPath, object arg = null)
            => await MaybeBody<T>(requestPath, "POST", arg);
        public static async Task<T> DeleteAsync<T>(string requestPath, object arg = null)
            => await MaybeBody<T>(requestPath, "DELETE", arg);
        public static async Task<T> PutAsync<T>(string requestPath, object arg = null)
            => await MaybeBody<T>(requestPath, "PUT", arg);

        public static async Task<T> MaybeBody<T>(string requestPath, string method, object arg = null)
          => arg == null
          ? await WithoutBody<T>(requestPath, method)
          : await WithBody<T>(requestPath, method, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(arg)));

        public static async Task<T> WithoutBody<T>(string requestPath, string method)
        {
            WebResponse response;
            return (await CreateRequest(requestPath, method).GetResponseAsync()).AsJson<T>();
        }
        public static async Task<T> WithBody<T>(string requestPath, string method, byte[] body)
        {
            WebResponse response;
            var request = CreateRequest(requestPath, method);
            using (var rs = await request.GetRequestStreamAsync())
                rs.Write(body, 0, body.Length);
            return (await request.GetResponseAsync()).AsJson<T>();
        }

        public static HttpWebRequest CreateRequest(string rp, string method)
        {
            var wr = WebRequest.CreateHttp(url + "/" + rp);// + "?token=" + Token);
            wr.Headers.Set("X-Token", Token);
            wr.ContentType = "application/json";
            wr.Accept = "application/json; charset=utf-8";

            wr.Method = method;
            return wr;
        }
        public static T AsJson<T>(this WebResponse response)
        {
            using (var r = new StreamReader(response.GetResponseStream()))
                return JsonConvert.DeserializeObject<T>(r.ReadToEnd());
        }

    }
}
