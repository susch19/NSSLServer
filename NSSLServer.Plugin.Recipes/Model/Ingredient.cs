using Newtonsoft.Json;
using System;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class Ingredient
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("unitId")]
        public string UnitId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("isBasic")]
        public bool IsBasic { get; set; }

        [JsonProperty("usageInfo")]
        public string UsageInfo { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("foodId")]
        public string FoodId { get; set; }

        [JsonProperty("productGroup")]
        public string ProductGroup { get; set; }

        [JsonProperty("blsKey")]
        public string BlsKey { get; set; }

        public override string ToString()
        {
            return Amount == 0 ? Name : $"{Name} {Amount}{Unit}";
        }
    }
}
