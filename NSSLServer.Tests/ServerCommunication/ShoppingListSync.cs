using NSSL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NSSL.ServerCommunication.HelperMethods;
using static Shared.RequestClasses;
using static Shared.ResultClasses;

namespace NSSLServer.Tests.ServerCommunication
{
    public static class ShoppingListSync
    {
        private static string path = "shoppinglists";


        public static async Task<ShoppingList> AddList(string listName)
            => await PostAsync<ShoppingList>($"{path}", new AddListArgs { Name = listName });

        public static async Task<ShoppingList> GetList(int listId)
            => await GetAsync<ShoppingList>($"{path}/{listId}/false");

        public static async Task<ChangeListNameResult> RenameList(int listId, string newName)
            => await PutAsync<ChangeListNameResult>($"{path}/{listId}", new ChangeListNameArgs { Name = newName });

        public static async Task<Result> DeleteList(int listId)
            => await DeleteAsync<Result>($"{path}/{listId}");


        public static async Task<AddListItemResult> AddProduct(int listId, string productName, string gtin, int amount)
            => await PostAsync<AddListItemResult>($"{path}/{listId}/products/", new AddProductArgs { Amount = amount, Gtin = gtin, ProductName = productName });

        public static async Task<GetContributorsResult> GetContributors(int listId)
            => await GetAsync<GetContributorsResult>($"{path}/{listId}/contributors/");


        public static async Task<ChangeListItemResult> ChangeProduct(int listId, int productId, int change, string newName)
            => await PutAsync<ChangeListItemResult>($"{path}/{listId}/products/{productId}", new ChangeProductArgs { Change = change, NewName = newName });

        public static async Task<Result> DeleteProduct(int listId, int productId)
            => await DeleteAsync<Result>($"{path}/{listId}/products/{productId}");


        public static async Task<AddContributorResult> AddContributor(int listId, string contributorName)
            => await PostAsync<AddContributorResult>($"{path}/{listId}/contributors/", new AddContributorArgs { Name = contributorName });

        public static async Task<Result> ChangeRights(int listId, int contributorId)
            => await PutAsync<Result>($"{path}/{listId}/contributors/{contributorId}", new object());

        public static async Task<Result> TransferOwnership(int listId, int newOwner)
            => await PutAsync<Result>($"{path}/{listId}/owners/", new TransferOwnershipArgs { NewOwnerId = newOwner });

        public static async Task<Result> DeleteContributor(int listId, int userId)
            => await DeleteAsync<Result>($"{path}/{listId}/contributors/{userId}");


        public static async Task<ListsResult> GetLists()
            => await GetAsync<ListsResult>($"{path}/batchaction/");

        public static async Task<HashResult> ChangeProducts(int listId, List<int> productIds, List<int> change)
            => await PostAsync<HashResult>($"{path}/{listId}/products/batchaction/change", new BatchProductArgs { ProductIds = productIds, Amount = change });
        public static async Task<DeleteProductsResult> DeleteProducts(int listId, List<int> productIds)
            => await PostAsync<DeleteProductsResult>($"{path}/{listId}/products/batchaction/delete", new BatchProductArgs { ProductIds = productIds });

        //public static async Task<ListsResult> GetLists(int listId)
        //    => await GetAsync<ListsResult>($"{path}/batchaction/");

    }
}