using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class ResultClasses
    {
        public class CreateResult
        {
            public bool Success;
            public string Error;
            public int Id { get; set; }
            public string Username { get; set; }
            public string EMail { get; set; }
        }
        public class LoginResult
        {
            public bool Success;
            public string Error;
            public int Id { get; set; }
            public string Username { get; set; }
            public string EMail { get; set; }
            public string Token { get; set; }
        }
        public class AddContributorResult
        {
            public bool Success;
            public string Error;
            public string Name { get; set; }
            public int Id { get; set; }
        }
        public class GetContributorsResult
        {
            public class ContributorResult
            {
                public string Name { get; set; }
                public int UserId { get; set; }
                public bool IsAdmin { get; set; }

            }
            public bool Success;
            public string Error;

            public List<ContributorResult> Contributors { get; set; }
        }

        public class ProductResult
        {
            public bool Success;
            public string Error;
            public string Name { get; set; }
            public string Gtin { get; set; }
            public decimal Quantity { get; set; }
            public string Unit { get; set; }
        }
        public class AddListItemResult
        {
            public bool Success;
            public string Error;
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Gtin { get; set; }
        }

        public class ChangeListItemResult
        {
            public bool Success;
            public string Error;
            public string Name { get; set; }
            public int Id { get; set; }
            public int Amount { get; set; }
            public int ListId { get; set; }
        }
        public class ChangeListNameResult
        {
            public bool Success;
            public string Error;
            public string Name { get; set; }
            public int ListId { get; set; }
        }
        public class AddListResult
        {
            public bool Success;
            public string Error;
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class ListsResult
        {
            public class ListResultItem
            {
                public string Name { get; set; }
                public bool IsAdmin { get; set; }
            }

            public Dictionary<int, ListResultItem> Lists { get; set; }
        }
        public class InfoResult
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string EMail { get; set; }
            public List<int> ListIds { get; set; }
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
