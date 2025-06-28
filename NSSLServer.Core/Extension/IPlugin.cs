using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace NSSLServer.Core.Extension
{

    /// <summary>
    /// Interface for plugins 
    /// </summary>
    /// <remarks>
    ///  Has to contain empty ctor, otherwise not loaded
    /// </remarks>
    public interface IPlugin
    {
        public string Name { get; }

        bool Initialize(NLog.LogFactory logFactory);

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment environment) { }
        public virtual void ConfigureServices(IServiceCollection services) { }
    }
}
