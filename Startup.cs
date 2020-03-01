using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ForwardedHeadersWebTester
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //Alt. set ASPNETCORE_FORWARDEDHEADERS_ENABLED to true but that would ONLY add 
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedHost |     //Not included in the defaults using ASPNETCORE_FORWARDEDHEADERS_ENABLED
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                options.ForwardLimit=2;
                options.KnownNetworks.Clear(); //In a real scenario we would add the real proxy network(s) here based on a config parameter
                options.KnownProxies.Clear();  //In a real scenario add the real proxy here based on a config parameter
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseForwardedHeaders();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    //Output the relevant properties as the framework sees it
                    await context.Response.WriteAsync($"---As the application sees it{Environment.NewLine}");
                    await context.Response.WriteAsync($"HttpContext.Connection.RemoteIpAddress : {context.Connection.RemoteIpAddress}{Environment.NewLine}");
                    await context.Response.WriteAsync($"HttpContext.Connection.RemoteIpPort : {context.Connection.RemotePort}{Environment.NewLine}");
                    await context.Response.WriteAsync($"HttpContext.Request.Scheme : {context.Request.Scheme}{Environment.NewLine}");
                    await context.Response.WriteAsync($"HttpContext.Request.Host : {context.Request.Host}{Environment.NewLine}");

                    //Output relevant request headers (starting with an X)
                    await context.Response.WriteAsync($"{Environment.NewLine}---Request Headers starting with X{Environment.NewLine}");
                    foreach (var header in context.Request.Headers.Where(h=>h.Key.StartsWith("X", StringComparison.OrdinalIgnoreCase )))
                    {
                        await context.Response.WriteAsync($"Request-Header {header.Key}: {header.Value}{Environment.NewLine}");
                    }
                });
            });
        }
    }
}
