using Microsoft.AspNetCore.Mvc;
using NSSLServer.Database;
using NSSLServer.Database.Attributes;
using NSSLServer.Plugin.Recipes.Model;
using NSSLServer.Plugin.Shoppinglist.Manager;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public async Task<IActionResult> CreateShoppingListForRecipe([FromBody] AddRecipeArgs idOrUrl, string? optionalListName = null, int? amountOfPeople = 4)
        {
            var recipe = await DownloadRecipe(idOrUrl.IdOrUrl);

            if (recipe is null)
                return NotFound(idOrUrl.IdOrUrl);

            var recipeName = recipe.Title;

            if (!string.IsNullOrWhiteSpace(optionalListName))
                recipeName = optionalListName;
            else if (string.IsNullOrWhiteSpace(recipeName))
                recipeName = $"Neues Rezept {DateTime.Now.ToShortDateString()}";

            var list = await ShoppingListManager.AddList(Context, recipeName, Session.Id);
            return await AddRecipeItemsToList(recipe, list.Id);
        }

        [HttpPost, Route(nameof(AddRecipeToList))]
        public async Task<IActionResult> AddRecipeToList([FromBody] AddRecipeArgs idOrUrl, int listId, int? amountOfPeople = 4)
        {
            var recipe = await DownloadRecipe(idOrUrl.IdOrUrl);

            if (recipe is null)
                return NotFound(idOrUrl.IdOrUrl);

            var list = await ShoppingListManager.LoadShoppingList(listId, false, Session.Id);

            if (list is null)
                return NotFound(listId);

            return await AddRecipeItemsToList(recipe, list.Id);
        }

        [HttpGet, Route(nameof(GetRecipe))]
        public async Task<IActionResult> GetRecipe(string urlOrId)
        {
            var recipe = await DownloadRecipe(urlOrId);

            return recipe is null ? NotFound(urlOrId) : Json(recipe);
        }

        private async Task<IActionResult> AddRecipeItemsToList(Recipe recipe, int listId)
        {
            foreach (var item in recipe.IngredientGroups.SelectMany(x => x.Ingredients))
            {
                if (item is null)
                    continue;

                var amount = item.Amount == 0 ? "" : item.Amount.ToString();

                await ShoppingListManager.AddProduct(Context, listId, Session.Id,
                    $"{item.Name} {amount} {item.Unit}{item.UsageInfo}".Trim(), null, 1, null);
            }

            return Json(await ShoppingListManager.LoadShoppingList(listId, false, Session.Id));
        }

        private async Task<Recipe?> DownloadRecipe(string idOrUrl)
        {
            var match = IdRegex.Match(idOrUrl);

            if (!match.Success)
                return default;

            var res = await _httpClient.GetFromJsonAsync<Recipe>($"{BaseUrl}{match.Value}");

            return res;
        }
    }
}
