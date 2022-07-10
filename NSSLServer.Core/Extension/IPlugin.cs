using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSSLServer.Core.Extension
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///  Has to contain emtpy ctor, otherwise not loaded
    /// </remarks>
    public interface IPlugin
    {
        public string Name { get; }

        bool Initialize(NLog.LogFactory logFactory);

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment environment) { }
        public virtual void ConfigureServices(IServiceCollection services) { }
    }
}
