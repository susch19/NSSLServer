using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NSSLServer.Features
{
    public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var type in Program.PluginLoader.ControllerTypes)
            {
                feature.Controllers.Add(type.GetTypeInfo());
            }
        }
    }
}
