using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class RequestClasses
    {
        public class LoginArgs
        {
            public string Username { get; set; }
            public string EMail { get; set; }
            public string PWHash { get; set; }
        }
        public class ChangePasswordArgs
        {
            public string OldPWHash { get; set; }
            public string NewPWHash { get; set; }

        }
        public class AddContributorArgs
        {
            public string Name { get; set; }
        }
        public class TransferOwnershipArgs
        {
            public int? NewOwnerId { get; set; }
        }
        public class ChangeProductArgs
        {
            public int? Change { get; set; }
            public string NewName { get; set; }
            public int? Order { get; set; }
        }
        public class ChangeListNameArgs
        {
            public string Name { get; set; }
        }
        public class AddListArgs
        {
            public string Name { get; set; }
        }
        public class BatchProductArgs
        {
            public List<int> ProductIds { get; set; }
            public List<int> Amount { get; set; }
        }
        public class AddProductArgs
        {
            public string ProductName { get; set; }
            public string Gtin { get; set; }
            public int? Amount { get; set; }
            public int? Order { get; set; }
        }
        public class AddNewProductArgs
        {
            public string Name { get; set; }
            public string Gtin { get; set; }
            public string Unit { get; set; }
            public decimal? Quantity { get; set; }
        }
        public class GetProductsArgs
        {
            public int Page { get; set; }
        }
    }
}
