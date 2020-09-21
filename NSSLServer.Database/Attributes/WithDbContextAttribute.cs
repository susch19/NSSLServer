using Microsoft.AspNetCore.Mvc.Filters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Database.Attributes
{
    public class WithDbContextAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var controller = (BaseDbContextController)context.Controller;
            using (controller.Context = new DBContext(await NsslEnvironment.OpenConnectionAsync(), true))
            {
                await next.Invoke();
                controller.Context.Connection.Close();
            }

            //await controller.Context.SaveChangesAsync();
        }
    }
}
