
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NSSLServer.Models.DatabaseConnection;
using Microsoft.AspNetCore.Mvc;

namespace NSSLServer.Module
{
    [Route("products")]
    public class ProductModule : AuthenticatingController
    {
        [HttpGet,Route("{id}")]
        public async Task<IActionResult> GetProduct([FromRoute]string id, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();
            long i;
            if ((id.Length == 8 || id.Length == 13) && long.TryParse(id, out i))
                return Json((await ProductSourceManager.FindProductByCode(id)));
            else
                return Json((await ProductSourceManager.FindProductsByName(id)));

        }
    }
}
