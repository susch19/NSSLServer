using Newtonsoft.Json;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class Rating
    {
        [JsonProperty("rating")]
        public double RatingRating { get; set; }

        [JsonProperty("numVotes")]
        public long NumVotes { get; set; }
    }
}
