using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyWebApp.IntegrationTests
{
    public class CustomRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPAddress _fakeIpAddress;

        public CustomRemoteIpAddressMiddleware(RequestDelegate next, IPAddress? fakeIpAddress = null)
        {
            _next = next;
            _fakeIpAddress = fakeIpAddress ?? IPAddress.Parse("127.0.0.1");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = _fakeIpAddress;
            await _next(httpContext);
        }
    }

    public class CustomRemoteIpStartupFilter : IStartupFilter
    {
        private readonly IPAddress? _remoteIp;

        public CustomRemoteIpStartupFilter(IPAddress? remoteIp = null)
        {
            _remoteIp = remoteIp;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<CustomRemoteIpAddressMiddleware>(_remoteIp);
                next(app);
            };
        }
    }
}
