using Newtonsoft.Json;

namespace Firebase
{
    public class Models
    {
        public class Notification
        {
            [JsonProperty("body")]
            public string Body { get; set; }
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("icon")]
            public string Icon { get; set; }
            [JsonProperty("click_action")]
            public string ClickAction { get; set; } = "FLUTTER_NOTIFICATION_CLICK";
        }
    }
    
    public enum Priority
    {
        normal,
        high
    }
}
