using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Firebase.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Firebase
{
    public class FirebaseCloudMessaging
    {
        public string ServerKey { get; set; }
        HttpClient client = new HttpClient();

        public FirebaseCloudMessaging(string serverKey) : base()
        {
            ServerKey = serverKey;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + ServerKey);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        public async Task<string> TopicMessage<T>(string topic, T payload, Notification notification = null, Priority priority = Priority.normal) =>
            await Send("/topics/" + topic, payload, notification, priority);

        public async Task<string> AppMessage<T>(string appName, T payload, Notification notification = null, Priority priority = Priority.normal) =>
            await Send(appName, payload, notification, priority);

        public async Task<string> UserMessage<T>(string userId, T payload, Notification notification = null, Priority priority = Priority.normal) =>
            await Send(userId, payload, notification, priority);

        private async Task<string> Send<T>(string to, T payload, Notification notification, Priority priority)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, new Uri("https://fcm.googleapis.com/fcm/send"));
                        
            string content = $"{{ \"data\": {JsonConvert.SerializeObject(payload)}, \"to\" : \"{to}\",";
            if (notification != null)
                content += $"\"notification\":{JsonConvert.SerializeObject(notification)},";
            content += $"\"priority\":\"{priority}\"}}"; 

            request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

            var res = await client.SendAsync(request);
            return res.ToString();
        }
    }
}
