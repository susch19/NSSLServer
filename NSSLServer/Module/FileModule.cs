using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace NSSLServer.Module
{
    [Route("file")]
    public class FileSend : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            var file = new FileStreamResult(System.IO.File.OpenRead(@"C:\Users\susch\Downloads\game\Peinkofer, Michael - Orks 05 - Die Ehre der Orks.epub"), "application/epub")
            {
                FileDownloadName = "Peinkofer, Michael - Orks 05 - Die Ehre der Orks.epub"
            };
            return file;
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously