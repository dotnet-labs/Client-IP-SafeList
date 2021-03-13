using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyWebApp.IntegrationTests
{
    public class CustomRemoteIpStartupFilter : IStartupFilter
    {
        private readonly IPAddress _ipAddress;

        public CustomRemoteIpStartupFilter(IPAddress ipAddress = null)
        {
            _ipAddress = ipAddress ?? IPAddress.Parse("127.0.0.1");
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseCustomRemoteIpAddressMiddleware(c => c.IpAddress = _ipAddress);
                next(app);
            };
        }
    }

    public static class CustomRemoteIpAddressMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomRemoteIpAddressMiddleware(this IApplicationBuilder app, Action<RemoteIpOptions> setupAction = null)
        {
            var options = new RemoteIpOptions
            {
                IpAddress = IPAddress.Parse("127.0.0.1")
            };
            setupAction?.Invoke(options);
            return app.UseMiddleware<CustomRemoteIpAddressMiddleware>(options);
        }
    }

    public class RemoteIpOptions
    {
        public IPAddress IpAddress { get; set; }
    }

    public class CustomRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPAddress _fakeIpAddress;

        public CustomRemoteIpAddressMiddleware(RequestDelegate next, RemoteIpOptions fakeIpAddress)
        {
            _next = next;
            _fakeIpAddress = fakeIpAddress.IpAddress;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = _fakeIpAddress;
            await _next(httpContext);
        }
    }
}
