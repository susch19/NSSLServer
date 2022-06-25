using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSSLServer.Database;
using NSSLServer.Database.Attributes;
using NSSLServer.Plugin.Shoppinglist.Manager;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static NSSLServer.Shared.RequestClasses;

namespace NSSLServer.Plugin.Recipes.Controller
{
    [Route("recipe"), WithDbContext]
    public class RecipeController : AuthenticatingDbContextController
    {
        private const string BaseUrl = "https://api.chefkoch.de/v2/recipes/";

        private static readonly Regex IdRegex = new(@"(\d{10,})", RegexOptions.IgnoreCase);

        private readonly HttpClient _httpClient;

        public RecipeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost, Route(nameof(CreateShoppingListForRecipe))]
        public async Task<IActionResult> CreateShoppingListForRecipe([FromBody] AddRecipeArgs urlOrId, string? optionalListName = null, int? amountOfPeople = 4)
        {
            var recipe = await DownloadRecipe(urlOrId.IdOrUrl);

            if (recipe is null)
                return NotFound(urlOrId.IdOrUrl);

            var recipeName = recipe.Title;

            if (!string.IsNullOrWhiteSpace(optionalListName))
                recipeName = optionalListName;
            else if (string.IsNullOrWhiteSpace(recipeName))
                recipeName = $"Neues Rezept {DateTime.Now.ToShortDateString()}";

            var list = await ShoppingListManager.AddList(Context, recipeName, Session.Id);
            foreach (var item in recipe.IngredientGroups.SelectMany(x => x.Ingredients))
            {
                if (item is null)
                    continue;

                var amount = item.Amount == 0 ? "" : item.Amount.ToString();

                await ShoppingListManager.AddProduct(Context, list.Id, Session.Id,
                    $"{item.Name} {amount} {item.Unit}{item.UsageInfo}".Trim(), null, 1, null);
            }

            return Json(await ShoppingListManager.LoadShoppingList(list.Id, false, Session.Id));
        }

        [HttpGet, Route(nameof(GetRecipe))]
        public async Task<IActionResult> GetRecipe(string urlOrId)
        {
            var recipe = await DownloadRecipe(urlOrId);

            return recipe is null ? NotFound(urlOrId) : Json(recipe);
        }

        private async Task<Recipes?> DownloadRecipe(string urlOrId)
        {
            var match = IdRegex.Match(urlOrId);

            if (!match.Success)
                return default;

            var res = await _httpClient.GetFromJsonAsync<Recipes>($"{BaseUrl}{match.Value}");

            return res;
        }
    }

    public class Recipes
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("difficulty")]
        public long Difficulty { get; set; }

        [JsonProperty("hasImage")]
        public bool HasImage { get; set; }

        [JsonProperty("hasVideo")]
        public bool HasVideo { get; set; }

        [JsonProperty("previewImageId")]
        public string PreviewImageId { get; set; }

        [JsonProperty("previewImageOwner")]
        public Owner PreviewImageOwner { get; set; }

        [JsonProperty("preparationTime")]
        public long PreparationTime { get; set; }

        [JsonProperty("isSubmitted")]
        public bool IsSubmitted { get; set; }

        [JsonProperty("isRejected")]
        public bool IsRejected { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("imageCount")]
        public long ImageCount { get; set; }

        [JsonProperty("editor")]
        public object Editor { get; set; }

        [JsonProperty("submissionDate")]
        public object SubmissionDate { get; set; }

        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("previewImageUrlTemplate")]
        public Uri PreviewImageUrlTemplate { get; set; }

        [JsonProperty("servings")]
        public long Servings { get; set; }

        [JsonProperty("kCalories")]
        public long KCalories { get; set; }

        [JsonProperty("nutrition")]
        public Nutrition Nutrition { get; set; }

        [JsonProperty("instructions")]
        public string Instructions { get; set; }

        [JsonProperty("miscellaneousText")]
        public string MiscellaneousText { get; set; }

        [JsonProperty("ingredientsText")]
        public string IngredientsText { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("fullTags")]
        public List<FullTag> FullTags { get; set; }

        [JsonProperty("viewCount")]
        public long ViewCount { get; set; }

        [JsonProperty("cookingTime")]
        public long CookingTime { get; set; }

        [JsonProperty("restingTime")]
        public long RestingTime { get; set; }

        [JsonProperty("totalTime")]
        public long TotalTime { get; set; }

        [JsonProperty("ingredientGroups")]
        public List<IngredientGroup> IngredientGroups { get; set; }

        [JsonProperty("categoryIds")]
        public List<string> CategoryIds { get; set; }

        [JsonProperty("recipeVideoId")]
        public string RecipeVideoId { get; set; }

        [JsonProperty("isIndexable")]
        public bool IsIndexable { get; set; }

        [JsonProperty("affiliateContent")]
        public string AffiliateContent { get; set; }

        [JsonProperty("isPlus")]
        public bool IsPlus { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; }
    }

    public class FullTag
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class IngredientGroup
    {
        [JsonProperty("header")]
        public string Header { get; set; }

        [JsonProperty("ingredients")]
        public List<Ingredient> Ingredients { get; set; }
    }

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

    public class Rating
    {
        [JsonProperty("rating")]
        public double RatingRating { get; set; }

        [JsonProperty("numVotes")]
        public long NumVotes { get; set; }
    }
}
