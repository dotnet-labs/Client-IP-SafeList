using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyWebApp.IntegrationTests
{
    public class CustomRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPAddress _fakeIpAddress;

        public CustomRemoteIpAddressMiddleware(RequestDelegate next, RemoteIpOptions options)
        {
            _next = next;
            _fakeIpAddress = options?.IpAddress ?? IPAddress.Parse("127.0.0.1");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = _fakeIpAddress;
            await _next(httpContext);
        }
    }

    public class CustomRemoteIpStartupFilter : IStartupFilter
    {
        private readonly RemoteIpOptions _remoteIpOptions;

        public CustomRemoteIpStartupFilter(IPAddress ipAddress = null)
        {
            _remoteIpOptions = new RemoteIpOptions
            {
                IpAddress = ipAddress ?? IPAddress.Parse("127.0.0.1")
            };
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<CustomRemoteIpAddressMiddleware>(_remoteIpOptions);
                next(app);
            };
        }
    }

    public class RemoteIpOptions
    {
        public IPAddress IpAddress { get; set; }
    }
}
