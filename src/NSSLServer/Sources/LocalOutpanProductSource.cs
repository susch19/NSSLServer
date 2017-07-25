﻿using NSSLServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Sources
{
    public class LocalCacheProductSource : IProductSource
    {
        public bool Islocal { get; } = true;

        public long Total { get; set; } = 0;

        public LocalCacheProductSource()
        {
        }

        public BasicProduct FindProductByCode(string code)
        {
            return null;
        }

        public BasicProduct[] FindProductsByName(string name)
        {
            return null;
        }

        private void InternalAddProduct(string name, string gtin)
        {
        }

        //public static async void AddProduct(string name, string gtin, int quantity = 0, string unit = null)
        //{
        //    //TODO Implement saving from Outpan
        //}

        Task<IDatabaseProduct> IProductSource.FindProductByCode(string code)
        {
            return null;
        }

        public Task<Paged<IDatabaseProduct>> FindProductsByName(string name, int page = 1)
        {
            return new Task<Paged<IDatabaseProduct>>(null);
        }
    }
}
