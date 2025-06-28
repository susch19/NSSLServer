using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;


namespace NSSLServer.Plugin.Example
{
    [Microsoft.AspNetCore.Mvc.Route("example")]
    public class TestEndpointPlugin : BaseController
    {
        private HttpClient client;

        public TestEndpointPlugin(HttpClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public async Task<IActionResult> TestHttpCall(string url, string contentType)
        {
            var res = await client.GetStringAsync(url);
            return new ContentResult() { Content = res, ContentType = contentType };
            //return Json(Tuple.Create("Hallo", "Welt", "vom Plugin", ":)"));
        }
    }
}
