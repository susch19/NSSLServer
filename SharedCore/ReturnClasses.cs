using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class ResultClasses
    {
        public class CreateResult : Result
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string EMail { get; set; }
        }
        public class LoginResult : Result
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string EMail { get; set; }
            public string Token { get; set; }
        }
        public class AddContributorResult : Result
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }
        public class GetContributorsResult : Result
        {
            public class ContributorResult
            {
                public string Name { get; set; }
                public int UserId { get; set; }
                public bool IsAdmin { get; set; }

            }

            public List<ContributorResult> Contributors { get; set; }
        }

        public class ProductResult : Result
        {
            public string Name { get; set; }
            public string Gtin { get; set; }
            public decimal? Quantity { get; set; }
            public string Unit { get; set; }
        }
        public class AddListItemResult : Result
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Gtin { get; set; }
        }

        public class ChangeListItemResult : Result
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public int Amount { get; set; }
            public int ListId { get; set; }
        }
        public class ChangeListNameResult : Result
        {
            public string Name { get; set; }
            public int ListId { get; set; }
        }
        public class AddListResult : Result
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class ShoppingListItemResult
        {
            public string Name { get; set; }
            public string Gtin { get; set; }
            public int Amount { get; set; }
            public int Id { get; set; }
        }
        public class ListsResult
        {
            public class ListResultItem
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public bool IsAdmin { get; set; }
                public List<ShoppingListItemResult> Products { get; set; }
            }

            public List<ListResultItem> Lists { get; set; }
        }
        public class InfoResult
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string EMail { get; set; }
            public List<int> ListIds { get; set; }
        }
        public class DeleteProductsResult : Result
        {
            public List<int> productIds { get; set; }
        }

        public class HashResult : Result
        {
            public int Hash { get; set; }
        }

        public class Result
        {
            public bool Success;
            public string Error;
        }

        public class Result<TItem>
        {
            public bool Success;
            public string Error;
            public TItem Item;
        }
        public class SessionRefreshResult
        {
            public string Token { get; set; }
        }
    }
}
