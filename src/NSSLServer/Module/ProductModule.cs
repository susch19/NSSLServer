
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NSSLServer.Models.DatabaseConnection;
using Microsoft.AspNetCore.Mvc;
using static Shared.RequestClasses;

namespace NSSLServer.Module
{
    [Route("products")]
    public class ProductModule : AuthenticatingController
    {
        [HttpGet,Route("{id}")]
        public async Task<IActionResult> GetProduct([FromRoute]string id, [FromQuery]int? page)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();
            if ((id.Length == 8 || id.Length == 13) && long.TryParse(id, out long i))
                return Json((await ProductSourceManager.FindProductByCode(id)));
            else
                return Json((await ProductSourceManager.FindProductsByName(id, page??1)));
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody]AddNewProductArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Gtin) || string.IsNullOrWhiteSpace(args.Name))
                return new BadRequestResult();
            if ((args.Gtin.Length == 8 || args.Gtin.Length == 13) && long.TryParse(args.Gtin, out long i))
                return Json((await ProductSourceManager.AddNewProduct(args.Gtin, args.Name)));
            else
                return new BadRequestResult();
        }
    }
}
