using ShipStation.Data;
using ShipStation.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vtex
{
    public class StartupExtender
    {
        public void ExtendConstructor(IConfiguration config, IWebHostEnvironment env)
        {

        }

        public void ExtendConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();
            services.AddSingleton<IShipStationRepository, ShipStationRepository>();
            services.AddSingleton<IShipStationAPIService, ShipStationAPIService>();
            services.AddSingleton<IVtexAPIService, VtexAPIService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddMemoryCache();
        }

        // This method is called inside Startup.Configure() before calling app.UseRouting()
        public void ExtendConfigureBeforeRouting(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        // This method is called inside Startup.Configure() before calling app.UseEndpoint()
        public void ExtendConfigureBeforeEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        // This method is called inside Startup.Configure() after calling app.UseEndpoint()
        public void ExtendConfigureAfterEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}