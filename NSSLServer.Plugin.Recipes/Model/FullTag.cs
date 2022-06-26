using Newtonsoft.Json;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class FullTag
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
