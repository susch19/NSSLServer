using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class IngredientGroup
    {
        [JsonProperty("header")]
        public string Header { get; set; }

        [JsonProperty("ingredients")]
        public List<Ingredient> Ingredients { get; set; }
    }
}
