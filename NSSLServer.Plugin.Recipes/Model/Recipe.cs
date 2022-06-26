using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable disable

namespace NSSLServer.Plugin.Recipes.Model
{
    public class Recipe
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
}
