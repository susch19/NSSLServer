using Newtonsoft.Json;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class Owner
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("hasAvatar")]
        public bool HasAvatar { get; set; }

        [JsonProperty("hasPaid")]
        public bool HasPaid { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }
}
