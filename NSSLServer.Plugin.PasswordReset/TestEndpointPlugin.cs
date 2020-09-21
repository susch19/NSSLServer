using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Shared.ResultClasses;

namespace NSSLServer.Plugin.Example
{
    [Microsoft.AspNetCore.Mvc.Route("example")]
    public class TestEndpointPlugin : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            return Json(Tuple.Create("Hallo", "Welt", "vom Plugin", ":)"));
        }
    }
}
