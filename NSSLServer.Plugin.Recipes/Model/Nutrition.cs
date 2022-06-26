using Newtonsoft.Json;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class Nutrition
    {
        [JsonProperty("kCalories")]
        public long KCalories { get; set; }

        [JsonProperty("carbohydrateContent")]
        public double CarbohydrateContent { get; set; }

        [JsonProperty("proteinContent")]
        public double ProteinContent { get; set; }

        [JsonProperty("fatContent")]
        public double FatContent { get; set; }
    }
}
