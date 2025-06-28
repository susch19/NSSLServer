﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSSLServer.Plugin.Shoppinglist.Manager;
using static NSSLServer.Shared.RequestClasses;

namespace NSSLServer.Plugin.Shoppinglist.Controller
{
    [Route("products")]
    public class ProductModule : AuthenticatingController
    {
        [HttpGet, Route("{id}")]
        public async Task<IActionResult> GetProduct([FromRoute]string id, [FromQuery]int? page)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();
            if ((id.Length == 8 || id.Length == 13) && long.TryParse(id, out long i))
                return Json((await ProductSourceManager.FindProductByCode(id)));
            else
                return Json((await ProductSourceManager.FindProductsByName(id, page ?? 1)));
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody]AddNewProductArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Gtin) || string.IsNullOrWhiteSpace(args.Name))
                return new BadRequestResult();
            if ((args.Gtin.Length == 8 || args.Gtin.Length == 13) && long.TryParse(args.Gtin, out long i))
                return Json((await ProductSourceManager.AddNewProduct(args.Gtin, args.Name, args.Quantity, args.Unit)));
            else
                return new BadRequestResult();
        }
    }
}
