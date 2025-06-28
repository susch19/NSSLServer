using FirebaseAdmin.Messaging;
using System.Linq;
using System.Threading.Tasks;

namespace NSSLServer.Plugin.Shoppinglist
{
    public static class FirebaseCloudMessaging
    {
        public static async Task TopicMessage<T>(string topicName, T payload)
        {
            var data = payload.GetType().GetProperties().ToDictionary(x => char.ToLowerInvariant(x.Name[0]) + x.Name.Substring(1), x => x.GetValue(payload).ToString());
            await FirebaseMessaging.DefaultInstance.SendAsync(new Message() { Topic = topicName, Data = data });
        }

        public static async Task TopicMessage(string topicName, string notificationTitle, string notificationBody)
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(new Message() { Topic = topicName, Notification = new Notification() { Title = notificationTitle, Body = notificationBody } });
        }
    }
}
